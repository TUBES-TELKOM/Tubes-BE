using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class PaymentServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _service = new PaymentService(_db, new PaymentStateMachine());

        SeedData();
    }

    private void SeedData()
    {
        _db.Menus.Add(new Menu
        {
            Id = 1,
            Name = "Nasi Goreng",
            Price = 25_000m,
            Category = "Makanan",
            IsAvailable = true
        });

        _db.Transactions.Add(new Transaction
        {
            Id = 1,
            TransactionCode = "TRX-TEST",
            CustomerName = "Budi",
            Status = Entities.Enums.TransactionStatus.Created,
            CreatedAt = DateTime.UtcNow,
            Items =
            [
                new TransactionItem
                {
                    Id = 1,
                    TransactionId = 1,
                    MenuId = 1,
                    Quantity = 2,
                    UnitPrice = 25_000m
                }
            ]
        });

        _db.SaveChanges();
    }

    [Fact]
    public async Task ProcessPayment_WithEnoughCash_ShouldCreatePayment()
    {
        var result = await _service.ProcessPaymentAsync(new PaymentRequest
        {
            TransactionId = 1,
            PaidAmount = 60_000m,
            PaymentMethod = "cash"
        });

        Assert.Equal(1, result.TransactionId);
        Assert.Equal(50_000m, result.TotalAmount);
        Assert.Equal(10_000m, result.ChangeAmount);
        Assert.Equal("cash", result.PaymentMethod);
        Assert.Equal("Completed", result.Status);

        var transaction = await _db.Transactions.Include(t => t.Payment).FirstAsync(t => t.Id == 1);
        Assert.NotNull(transaction.Payment);
        Assert.Equal("cash", transaction.PaymentMethod);
        Assert.Equal("Completed", transaction.Payment!.Status.ToString());

        var history = await _db.TransactionHistories.FirstAsync(h => h.TransactionId == 1);
        Assert.Equal(50_000m, history.TotalAmount);
        Assert.Equal("cash", history.PaymentMethod);
    }

    [Fact]
    public async Task ProcessPayment_WithInsufficientCash_ShouldThrow()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessPaymentAsync(new PaymentRequest
        {
            TransactionId = 1,
            PaidAmount = 40_000m,
            PaymentMethod = "cash"
        }));

        Assert.Contains("Uang tidak cukup", ex.Message);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
