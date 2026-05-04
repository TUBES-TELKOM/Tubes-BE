using Microsoft.AspNetCore.Mvc;
using Tubes_POS_API.Models;

namespace Tubes_POS_API.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
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
