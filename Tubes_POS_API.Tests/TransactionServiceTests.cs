using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Entities.Enums;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

/// <summary>
/// Unit tests untuk TransactionService versi POS Warung.
/// Fokus: Checkout sekaligus (CreateTransaction dengan Items),
/// perhitungan total otomatis dari backend.
/// </summary>
public class TransactionServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly TransactionService _service;

    public TransactionServiceTests()
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
        _db.Menus.AddRange(
            new Menu { Id = 1, Name = "Nasi Goreng", Price = 25_000m, Category = "Makanan", IsAvailable = true },
            new Menu { Id = 2, Name = "Es Teh", Price = 5_000m, Category = "Minuman", IsAvailable = true },
            new Menu { Id = 3, Name = "Ayam Bakar", Price = 35_000m, Category = "Makanan", IsAvailable = true },
            new Menu { Id = 4, Name = "Jus Alpukat", Price = 15_000m, Category = "Minuman", IsAvailable = false }
        );
        _db.SaveChanges();
    }

    // =========================================================================
    // TEST: CREATE TRANSACTION (CHECKOUT)
    // =========================================================================

    [Fact]
    public async Task CreateTransaction_WithValidItems_ShouldCalculateTotalCorrectly()
    {
        var request = new CreateTransactionRequest
        {
            CustomerName = "Budi",
            Items = new List<TransactionItemRequest>
            {
                new() { MenuId = 1, Quantity = 2 }, // 2 * 25.000 = 50.000
                new() { MenuId = 2, Quantity = 3 }  // 3 * 5.000 = 15.000
                                                    // Total = 65.000
            },
            PaidAmount = 100_000m,
            PaymentMethod = "cash"
        };

        var result = await _service.CreateTransactionAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Budi", result.CustomerName);
        Assert.Equal(65_000m, result.TotalAmount);
        Assert.Equal(100_000m, result.PaidAmount);
        Assert.Equal(35_000m, result.Change);
        Assert.Equal("cash", result.PaymentMethod);
        Assert.Equal("Completed", result.Status);
        Assert.Equal(2, result.Items.Count);
        Assert.StartsWith("TRX-", result.TransactionCode);
    }

    [Fact]
    public async Task CreateTransaction_MenuNotFound_ShouldThrowKeyNotFoundException()
    {
        var request = new CreateTransactionRequest
        {
            CustomerName = "Andi",
            Items = new List<TransactionItemRequest>
            {
                new() { MenuId = 99, Quantity = 1 } // Menu tidak ada
            }
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CreateTransactionAsync(request));
    }

    [Fact]
    public async Task CreateTransaction_MenuUnavailable_ShouldThrowArgumentException()
    {
        var request = new CreateTransactionRequest
        {
            CustomerName = "Cici",
            Items = new List<TransactionItemRequest>
            {
                new() { MenuId = 4, Quantity = 1 } // Jus Alpukat (IsAvailable = false)
            }
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateTransactionAsync(request));

        Assert.Contains("tidak tersedia", ex.Message);
    }

    // =========================================================================
    // TEST: TRANSACTION NOT FOUND
    // =========================================================================

    [Fact]
    public async Task GetById_NotFound_ShouldThrowKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.GetTransactionByIdAsync(999));
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
