using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Entities.Enums;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

/// <summary>
/// Unit tests untuk TransactionService.
/// Fokus: kalkulasi total, validasi operasi berdasarkan status (table-driven),
/// dan CRUD cart items.
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
    // TEST: CREATE TRANSACTION
    // =========================================================================

    [Fact]
    public async Task CreateTransaction_ShouldReturnNewTransaction_WithStatusCreated()
    {
        var request = new CreateTransactionRequest
        {
            CustomerName = "Budi",
            TableNumber = "A1"
        };

        var result = await _service.CreateTransactionAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Budi", result.CustomerName);
        Assert.Equal("A1", result.TableNumber);
        Assert.Equal("Created", result.Status);
        Assert.Equal(0, result.TotalAmount);
        Assert.StartsWith("TRX-", result.TransactionCode);
    }

    [Fact]
    public async Task CreateTransaction_ShouldGenerateUniqueCode()
    {
        var tx1 = await _service.CreateTransactionAsync(new CreateTransactionRequest());
        await Task.Delay(1); // Ensure different timestamp
        var tx2 = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        Assert.NotEqual(tx1.TransactionCode, tx2.TransactionCode);
    }

    // =========================================================================
    // TEST: TOTAL CALCULATION (UNIT TEST WAJIB)
    // =========================================================================

    [Fact]
    public async Task CalculateTotal_SingleItem_ShouldBeQuantityTimesPrice()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());
        var result = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 3 });

        // 3 x 25.000 = 75.000
        Assert.Equal(75_000m, result.TotalAmount);
    }

    [Fact]
    public async Task CalculateTotal_MultipleItems_ShouldSumAllSubtotals()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 2 }); // 2 x 25.000 = 50.000
        await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 2, Quantity = 3 }); // 3 x 5.000  = 15.000
        var result = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 3, Quantity = 1 }); // 1 x 35.000 = 35.000

        // Total = 50.000 + 15.000 + 35.000 = 100.000
        Assert.Equal(100_000m, result.TotalAmount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task CalculateTotal_AfterRemoveItem_ShouldRecalculate()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        var afterAdd1 = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 2 }); // 50.000
        await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 2, Quantity = 3 }); // 15.000
        // Total saat ini = 65.000

        // Hapus Nasi Goreng
        var itemToRemove = afterAdd1.Items.First(i => i.MenuId == 1);
        var result = await _service.RemoveItemAsync(tx.Id, itemToRemove.Id);

        // Total sekarang hanya Es Teh: 15.000
        Assert.Equal(15_000m, result.TotalAmount);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task CalculateTotal_AfterUpdateQuantity_ShouldRecalculate()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());
        var afterAdd = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 2 }); // 50.000

        var item = afterAdd.Items.First();
        var result = await _service.UpdateItemQuantityAsync(tx.Id, item.Id, new UpdateItemRequest { Quantity = 5 });

        // 5 x 25.000 = 125.000
        Assert.Equal(125_000m, result.TotalAmount);
    }

    [Fact]
    public async Task CalculateTotal_EmptyCart_ShouldBeZero()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());
        var afterAdd = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 1 });

        var item = afterAdd.Items.First();
        var result = await _service.RemoveItemAsync(tx.Id, item.Id);

        Assert.Equal(0m, result.TotalAmount);
        Assert.Empty(result.Items);
    }

    // =========================================================================
    // TEST: ADD ITEM — SAME MENU SHOULD MERGE QUANTITY
    // =========================================================================

    [Fact]
    public async Task AddItem_SameMenu_ShouldMergeQuantity()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 2 });
        var result = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 3 });

        // Harus tetap 1 item, quantity = 5
        Assert.Single(result.Items);
        Assert.Equal(5, result.Items.First().Quantity);
        Assert.Equal(125_000m, result.TotalAmount); // 5 x 25.000
    }

    // =========================================================================
    // TEST: VALIDATION — MENU NOT FOUND / UNAVAILABLE
    // =========================================================================

    [Fact]
    public async Task AddItem_MenuNotFound_ShouldThrowKeyNotFoundException()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 999, Quantity = 1 }));
    }

    [Fact]
    public async Task AddItem_MenuUnavailable_ShouldThrowArgumentException()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 4, Quantity = 1 }));

        Assert.Contains("tidak tersedia", ex.Message);
    }

    // =========================================================================
    // TEST: TABLE-DRIVEN — OPERASI BERDASARKAN STATUS
    // =========================================================================

    [Fact]
    public async Task AddItem_WhenStatusPaid_ShouldThrowInvalidOperationException()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        // Ubah status manual ke Paid untuk testing
        var entity = await _db.Transactions.FindAsync(tx.Id);
        entity!.Status = TransactionStatus.Paid;
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 1 }));

        Assert.Contains("sudah dibayar", ex.Message);
    }

    [Fact]
    public async Task RemoveItem_WhenStatusCompleted_ShouldThrowInvalidOperationException()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());
        var afterAdd = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 1 });

        // Ubah status ke Completed
        var entity = await _db.Transactions.FindAsync(tx.Id);
        entity!.Status = TransactionStatus.Completed;
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RemoveItemAsync(tx.Id, afterAdd.Items.First().Id));

        Assert.Contains("sudah selesai", ex.Message);
    }

    [Fact]
    public async Task UpdateItem_WhenStatusCancelled_ShouldThrowInvalidOperationException()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());
        var afterAdd = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 1 });

        // Ubah status ke Cancelled
        var entity = await _db.Transactions.FindAsync(tx.Id);
        entity!.Status = TransactionStatus.Cancelled;
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateItemQuantityAsync(tx.Id, afterAdd.Items.First().Id, new UpdateItemRequest { Quantity = 5 }));

        Assert.Contains("sudah dibatalkan", ex.Message);
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

    // =========================================================================
    // TEST: ITEM NOT FOUND IN TRANSACTION
    // =========================================================================

    [Fact]
    public async Task RemoveItem_ItemNotFound_ShouldThrowKeyNotFoundException()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.RemoveItemAsync(tx.Id, 999));
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
