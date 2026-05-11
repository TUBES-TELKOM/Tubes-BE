using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class HistoryReportTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HistoryService _historyService;
    private readonly ReportService _reportService;

    public HistoryReportTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _historyService = new HistoryService(_db);
        _reportService = new ReportService(_historyService);

        SeedHistoryData();
    }

    private void SeedHistoryData()
    {
        var now = DateTime.UtcNow;

        _db.TransactionHistories.AddRange(
            new TransactionHistory
            {
                Id = 1,
                TransactionId = 1,
                TransactionDate = now.AddDays(-2),
                PaymentMethod = "cash",
                TotalAmount = 50_000m
            },
            new TransactionHistory
            {
                Id = 2,
                TransactionId = 2,
                TransactionDate = now.AddDays(-1),
                PaymentMethod = "qris",
                TotalAmount = 75_000m
            },
            new TransactionHistory
            {
                Id = 3,
                TransactionId = 3,
                TransactionDate = now,
                PaymentMethod = "cash",
                TotalAmount = 25_000m
            }
        );

        _db.SaveChanges();
    }

    // Tests that history is returned in newest-first order.
    [Fact]
    public async Task GetAllAsync_ShouldReturnHistoryInDescendingOrder()
    {
        var result = await _historyService.GetAllAsync();

        Assert.Equal(3, result.Count);
        Assert.True(result[0].TransactionDate >= result[1].TransactionDate);
    }

    // Tests that date filtering works correctly.
    [Fact]
    public async Task GetByDateRangeAsync_ShouldFilterCorrectly()
    {
        var start = DateTime.UtcNow.AddDays(-1.5);
        var end = DateTime.UtcNow.AddHours(1);

        var result = await _historyService.GetByDateRangeAsync(start, end);

        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.True(item.TransactionDate >= start && item.TransactionDate <= end));
    }

    // Tests that payment-method filtering works correctly.
    [Fact]
    public async Task GetByPaymentMethodAsync_ShouldFilterCorrectly()
    {
        var result = await _historyService.GetByPaymentMethodAsync("cash");

        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.Equal("cash", item.PaymentMethod));
    }

    // Tests that report aggregation returns correct totals.
    [Fact]
    public async Task GetReportAsync_ShouldAggregateTotals()
    {
        var result = await _reportService.GetReportAsync(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddHours(1));

        var type = result.GetType();
        Assert.Equal(3, (int)type.GetProperty("TotalTransaksi")!.GetValue(result)!);
        Assert.Equal(150_000m, (decimal)type.GetProperty("TotalPendapatan")!.GetValue(result)!);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
