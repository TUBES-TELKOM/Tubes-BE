using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Controllers;
using Tubes_POS_API.Models;
using Tubes_POS_API.Entities;

namespace Tubes_POS_API.Tests;

public class HealthControllerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _controller = new HealthController(_db);
    }

    // Tests that general health reports database readiness.
    [Fact]
    public async Task Get_ShouldReturnHealthyResponse()
    {
        var result = await _controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<HealthCheckResponse>>(ok.Value);

        Assert.True(response.Success);
        Assert.Equal("Service is healthy.", response.Message);
        Assert.Equal("health", response.Data!.Probe);
        Assert.Equal("ready", response.Data.Checks["database"]);
    }

    // Tests that live endpoint only reports application liveness.
    [Fact]
    public void Live_ShouldReturnAliveResponse()
    {
        var result = _controller.Live();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<HealthCheckResponse>>(ok.Value);

        Assert.Equal("alive", response.Data!.Status);
        Assert.Equal("live", response.Data.Probe);
        Assert.Equal("running", response.Data.Checks["application"]);
    }

    // Tests that readiness endpoint returns ready state.
    [Fact]
    public async Task Ready_ShouldReturnReadyResponse()
    {
        var result = await _controller.Ready();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<HealthCheckResponse>>(ok.Value);

        Assert.Equal("ready", response.Data!.Status);
        Assert.Equal("ready", response.Data.Probe);
        Assert.Equal("ready", response.Data.Checks["database"]);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
