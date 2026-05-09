using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class TransactionPerformanceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly TransactionService _service;

    public TransactionPerformanceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _service = new TransactionService(_db);

        SeedMenuData();
    }

    private void SeedMenuData()
    {
        for (int i = 1; i <= 50; i++)
        {
            _db.Menus.Add(new Menu
            {
                Id = i,
                Name = $"Menu Item {i}",
                Price = 10_000m + (i * 1_000m),
                Category = i % 2 == 0 ? "Makanan" : "Minuman",
                IsAvailable = true,
            });
        }

        _db.SaveChanges();
    }

    [Fact]
    public async Task Checkout_50Items_ShouldCompleteUnder1Second()
    {
        var items = new List<TransactionItemRequest>();
        decimal expectedTotal = 0m;

        for (int i = 1; i <= 50; i++)
        {
            items.Add(new TransactionItemRequest { MenuId = i, Quantity = i });
            decimal price = 10_000m + (i * 1_000m);
            decimal tax = price * 0.11m;
            expectedTotal += (price + tax) * i;
        }

        var request = new CreateTransactionRequest
        {
            CustomerName = "Perf Test",
            Items = items,
            PaidAmount = expectedTotal + 50_000m,
            PaymentMethod = "qris"
        };

        var stopwatch = Stopwatch.StartNew();

        var result = await _service.CreateTransactionAsync(request);

        stopwatch.Stop();

        Assert.Equal(50, result.Items.Count);
        Assert.Equal(expectedTotal, result.TotalAmount);
        Assert.Equal(50_000m, result.Change);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1),
            $"Checkout 50 item memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 1000ms)");
    }

    [Fact]
    public async Task CreateManyTransactions_100Transactions_ShouldCompleteUnder3Seconds()
    {
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 100; i++)
        {
            await _service.CreateTransactionAsync(new CreateTransactionRequest
            {
                CustomerName = $"Customer {i}"
            });
        }

        stopwatch.Stop();

        var all = await _service.GetAllTransactionsAsync();
        Assert.Equal(100, all.Count);

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(3),
            $"Membuat 100 transaksi memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 3000ms)");
    }

    [Fact]
    public async Task CartOperations_MixedOps_ShouldCompleteUnder2Seconds()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        var stopwatch = Stopwatch.StartNew();

        for (int i = 1; i <= 20; i++)
        {
            await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = i, Quantity = 2 });
        }

        var currentTx = await _service.GetTransactionByIdAsync(tx.Id);
        foreach (var item in currentTx.Items.Take(10))
        {
            await _service.UpdateItemQuantityAsync(tx.Id, item.Id, new UpdateItemRequest { Quantity = 5 });
        }

        currentTx = await _service.GetTransactionByIdAsync(tx.Id);
        foreach (var item in currentTx.Items.TakeLast(5))
        {
            await _service.RemoveItemAsync(tx.Id, item.Id);
        }

        stopwatch.Stop();

        var result = await _service.GetTransactionByIdAsync(tx.Id);
        Assert.Equal(15, result.Items.Count);

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2),
            $"Mixed cart operations memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 2000ms)");
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
