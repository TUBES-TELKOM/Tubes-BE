using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

/// <summary>
/// Performance tests untuk TransactionService.
/// Mengukur waktu eksekusi operasi transaksi di bawah beban.
/// </summary>
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

    // =========================================================================
    // PERFORMANCE: TRANSAKSI DENGAN BANYAK ITEM
    // =========================================================================

    [Fact]
    public async Task AddManyItems_50Items_ShouldCompleteUnder2Seconds()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest
        {
            CustomerName = "Perf Test",
            TableNumber = "P1",
        });

        var stopwatch = Stopwatch.StartNew();

        for (int i = 1; i <= 50; i++)
        {
            await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = i, Quantity = i });
        }

        stopwatch.Stop();

        var result = await _service.GetTransactionByIdAsync(tx.Id);

        // Verifikasi semua item masuk
        Assert.Equal(50, result.Items.Count);

        // Verifikasi total benar: SUM(i * (10000 + i*1000)) for i=1..50
        decimal expectedTotal = 0;
        for (int i = 1; i <= 50; i++)
        {
            expectedTotal += i * (10_000m + (i * 1_000m));
        }
        Assert.Equal(expectedTotal, result.TotalAmount);

        // Performance: harus selesai di bawah 2 detik
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2),
            $"Menambah 50 item memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 2000ms)");
    }

    // =========================================================================
    // PERFORMANCE: BUAT BANYAK TRANSAKSI
    // =========================================================================

    [Fact]
    public async Task CreateManyTransactions_100Transactions_ShouldCompleteUnder3Seconds()
    {
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 100; i++)
        {
            await _service.CreateTransactionAsync(new CreateTransactionRequest
            {
                CustomerName = $"Customer {i}",
                TableNumber = $"T{i}",
            });
        }

        stopwatch.Stop();

        var all = await _service.GetAllTransactionsAsync();
        Assert.Equal(100, all.Count);

        // Performance: harus selesai di bawah 3 detik
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(3),
            $"Membuat 100 transaksi memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 3000ms)");
    }

    // =========================================================================
    // PERFORMANCE: OPERASI CART (ADD, UPDATE, REMOVE) BERURUTAN
    // =========================================================================

    [Fact]
    public async Task CartOperations_MixedOps_ShouldCompleteUnder2Seconds()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        var stopwatch = Stopwatch.StartNew();

        // Tambah 20 item
        for (int i = 1; i <= 20; i++)
        {
            await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = i, Quantity = 2 });
        }

        // Update quantity 10 item pertama
        var currentTx = await _service.GetTransactionByIdAsync(tx.Id);
        foreach (var item in currentTx.Items.Take(10))
        {
            await _service.UpdateItemQuantityAsync(tx.Id, item.Id, new UpdateItemRequest { Quantity = 5 });
        }

        // Hapus 5 item terakhir
        currentTx = await _service.GetTransactionByIdAsync(tx.Id);
        foreach (var item in currentTx.Items.TakeLast(5))
        {
            await _service.RemoveItemAsync(tx.Id, item.Id);
        }

        stopwatch.Stop();

        var result = await _service.GetTransactionByIdAsync(tx.Id);
        Assert.Equal(15, result.Items.Count);

        // Performance: harus selesai di bawah 2 detik
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2),
            $"Mixed cart operations memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 2000ms)");
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
