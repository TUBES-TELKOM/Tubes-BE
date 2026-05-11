using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

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

    // Tests that creating a transaction starts as an empty draft/cart.
    [Fact]
    public async Task CreateTransaction_EmptyRequest_ShouldCreateDraftTransaction()
    {
        var result = await _service.CreateTransactionAsync(new CreateTransactionRequest
        {
            CustomerName = "Budi",
            TableNumber = "A1"
        });

        Assert.Equal("Budi", result.CustomerName);
        Assert.Equal("A1", result.TableNumber);
        Assert.Equal("Created", result.Status);
        Assert.Equal(0m, result.TotalAmount);
        Assert.Empty(result.Items);
        Assert.StartsWith("TRX-", result.TransactionCode);
    }

    // Tests that cart items increase total amount correctly.
    [Fact]
    public async Task AddItem_ShouldCalculateTotalCorrectly()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest { CustomerName = "Budi" });

        await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 2 });
        var result = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 2, Quantity = 3 });

        Assert.Equal(72_150m, result.TotalAmount);
        Assert.Equal(2, result.Items.Count);
    }

    // Tests that adding the same menu twice merges quantity.
    [Fact]
    public async Task AddItem_SameMenu_ShouldMergeQuantity()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 2 });
        var result = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 3 });

        Assert.Single(result.Items);
        Assert.Equal(5, result.Items.First().Quantity);
        Assert.Equal(138_750m, result.TotalAmount);
    }

    // Tests that removing an item recalculates the total.
    [Fact]
    public async Task RemoveItem_ShouldRecalculateTotal()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        var afterAdd = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 2 });
        await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 2, Quantity = 3 });

        var itemToRemove = afterAdd.Items.First(i => i.MenuId == 1);
        var result = await _service.RemoveItemAsync(tx.Id, itemToRemove.Id);

        Assert.Equal(16_650m, result.TotalAmount);
        Assert.Single(result.Items);
    }

    // Tests that updating quantity recalculates the total.
    [Fact]
    public async Task UpdateItemQuantity_ShouldRecalculateTotal()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());
        var afterAdd = await _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 1, Quantity = 2 });

        var item = afterAdd.Items.First();
        var result = await _service.UpdateItemQuantityAsync(tx.Id, item.Id, new UpdateItemRequest { Quantity = 5 });

        Assert.Equal(138_750m, result.TotalAmount);
    }

    // Tests that unavailable menu items are rejected.
    [Fact]
    public async Task AddItem_MenuUnavailable_ShouldThrowArgumentException()
    {
        var tx = await _service.CreateTransactionAsync(new CreateTransactionRequest());

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AddItemAsync(tx.Id, new AddItemRequest { MenuId = 4, Quantity = 1 }));

        Assert.Contains("tidak tersedia", ex.Message);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
