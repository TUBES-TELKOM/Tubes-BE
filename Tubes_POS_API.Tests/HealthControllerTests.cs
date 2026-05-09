using Microsoft.AspNetCore.Mvc;
using Tubes_POS_API.Controllers;
using Tubes_POS_API.Models;

namespace Tubes_POS_API.Tests;

public class HealthControllerTests
{
    private readonly HealthController _controller = new();

    [Fact]
    public void Get_ShouldReturnHealthyResponse()
    {
        var result = _controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<HealthCheckResponse>>(ok.Value);

        Assert.True(response.Success);
        Assert.Equal("Service is healthy.", response.Message);
        Assert.Equal("health", response.Data!.Probe);
    }

    [Fact]
    public void Live_ShouldReturnAliveResponse()
    {
        var result = _controller.Live();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<HealthCheckResponse>>(ok.Value);

        Assert.Equal("alive", response.Data!.Status);
        Assert.Equal("live", response.Data.Probe);
    }

    [Fact]
    public void Ready_ShouldReturnReadyResponse()
    {
        var result = _controller.Ready();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<HealthCheckResponse>>(ok.Value);

        Assert.Equal("ready", response.Data!.Status);
        Assert.Equal("ready", response.Data.Probe);
    }
}
