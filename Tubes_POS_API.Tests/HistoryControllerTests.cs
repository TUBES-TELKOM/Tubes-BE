using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Controllers;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Models;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class HistoryControllerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HistoryController _controller;

    public HistoryControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        var historyService = new HistoryService(_db);
        var reportService = new ReportService(historyService);
        _controller = new HistoryController(historyService, reportService);

        _db.TransactionHistories.Add(new TransactionHistory
        {
            Id = 1,
            TransactionId = 1,
            TransactionDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            TotalAmount = 50_000m
        });
        _db.SaveChanges();
    }

    // Tests that history list response is wrapped.
    [Fact]
    public async Task GetAll_ShouldReturnWrappedResponse()
    {
        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<TransactionHistoryResponse>>>(ok.Value);

        Assert.True(response.Success);
        Assert.Single(response.Data!);
    }

    // Tests that report response is wrapped.
    [Fact]
    public async Task GetReport_ShouldReturnWrappedResponse()
    {
        var result = await _controller.GetReport(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ReportResponse>>(ok.Value);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
