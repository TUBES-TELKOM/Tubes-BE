using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

/// <summary>
/// Performance tests untuk TransactionService versi POS Warung.
/// Mengukur waktu eksekusi operasi checkout dalam satu hit.
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
    // PERFORMANCE: TRANSAKSI DENGAN BANYAK ITEM (SATU KALI HIT)
    // =========================================================================

    [Fact]
    public async Task Checkout_50Items_ShouldCompleteUnder1Second()
    {
        var items = new List<TransactionItemRequest>();
        decimal expectedTotal = 0;

        for (int i = 1; i <= 50; i++)
        {
            items.Add(new TransactionItemRequest { MenuId = i, Quantity = i });
            expectedTotal += i * (10_000m + (i * 1_000m));
        }

        var request = new CreateTransactionRequest
        {
            CustomerName = "Perf Test",
            Items = items,
            PaidAmount = expectedTotal + 50000m,
            PaymentMethod = "qris"
        };

        var stopwatch = Stopwatch.StartNew();

        var result = await _service.CreateTransactionAsync(request);

        stopwatch.Stop();

        // Verifikasi semua item masuk
        Assert.Equal(50, result.Items.Count);
        Assert.Equal(expectedTotal, result.TotalAmount);
        Assert.Equal(50000m, result.Change);

        // Performance: Karena cuma 1x hit DB save, harus sangat cepat (< 1 detik)
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1),
            $"Checkout 50 item memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 1000ms)");
    }

    // =========================================================================
    // PERFORMANCE: BUAT BANYAK TRANSAKSI (CONCURRENT/LOOP)
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
                Items = new List<TransactionItemRequest>
                {
                    new() { MenuId = 1, Quantity = 1 }
                },
                PaidAmount = 50_000m,
                PaymentMethod = "cash"
            });
        }

        stopwatch.Stop();

        var all = await _service.GetAllTransactionsAsync();
        Assert.Equal(100, all.Count);

        // Performance: harus selesai di bawah 3 detik
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(3),
            $"Membuat 100 transaksi memakan waktu {stopwatch.ElapsedMilliseconds}ms (batas: 3000ms)");
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
