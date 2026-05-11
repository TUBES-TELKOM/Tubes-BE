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

    // Tests cart add performance with many items.
    [Fact]
    public async Task AddManyItems_50Items_ShouldCompleteUnder1Second()
    {
        var transaction = await _service.CreateTransactionAsync(new CreateTransactionRequest
        {
            CustomerName = "Perf Test"
        });

        var stopwatch = Stopwatch.StartNew();

        for (int i = 1; i <= 50; i++)
        {
            await _service.AddItemAsync(transaction.Id, new AddItemRequest { MenuId = i, Quantity = i });
        }

        stopwatch.Stop();

        var result = await _service.GetTransactionByIdAsync(transaction.Id);

        decimal expectedTotal = 0m;
        for (int i = 1; i <= 50; i++)
        {
            var price = 10_000m + (i * 1_000m);
            expectedTotal += i * (price + (price * 0.11m));
        }

        Assert.Equal(50, result.Items.Count);
        Assert.Equal(expectedTotal, result.TotalAmount);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1),
            $"Add 50 item memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 1000ms)");
    }

    // Tests bulk transaction creation performance.
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

    // Tests mixed cart operations performance.
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
