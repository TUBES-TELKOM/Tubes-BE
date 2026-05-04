using Microsoft.AspNetCore.Mvc;
using Tubes_POS_API.Models;

namespace Tubes_POS_API.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    // TODO: replace with aggregated service status once dependencies are available.
    // Should report overall application health and summarize the result of each probe.
    public ActionResult<ApiResponse<HealthCheckResponse>> Get()
    {
        return Ok(new ApiResponse<HealthCheckResponse>
        {
            Message = "Service is healthy.",
            Data = new HealthCheckResponse
            {
                Status = "ok",
                Probe = "health"
            }
        });
    }

    [HttpGet("live")]
    // TODO: keep this probe lightweight.
    // It should only verify the application process is running and able to respond to requests.
    public ActionResult<ApiResponse<HealthCheckResponse>> Live()
    {
        return Ok(new ApiResponse<HealthCheckResponse>
        {
            Message = "Service is alive.",
            Data = new HealthCheckResponse
            {
                Status = "alive",
                Probe = "live"
            }
        });
    }

    [HttpGet("ready")]
    // TODO: expand this probe when infrastructure is added.
    // It should check required dependencies such as database connectivity, cache availability,
    // and any external services before the app is considered ready to receive traffic.
    public ActionResult<ApiResponse<HealthCheckResponse>> Ready()
    {
        return Ok(new ApiResponse<HealthCheckResponse>
        {
            Message = "Service is ready.",
            Data = new HealthCheckResponse
            {
                Status = "ready",
                Probe = "ready"
            }
        });
    }
}
