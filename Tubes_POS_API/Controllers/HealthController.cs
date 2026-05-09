using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Models;

namespace Tubes_POS_API.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    private readonly AppDbContext _db;

    public HealthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<HealthCheckResponse>>> Get()
    {
        var databaseReady = await _db.Database.CanConnectAsync();
        var status = databaseReady ? "ok" : "degraded";

        return Ok(new ApiResponse<HealthCheckResponse>
        {
            Message = databaseReady
                ? "Service is healthy."
                : "Service is degraded.",
            Data = new HealthCheckResponse
            {
                Status = status,
                Probe = "health",
                Checks = new Dictionary<string, string>
                {
                    ["database"] = databaseReady ? "ready" : "not-ready"
                }
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
                Probe = "live",
                Checks = new Dictionary<string, string>
                {
                    ["application"] = "running"
                }
            }
        });
    }

    [HttpGet("ready")]
    public async Task<ActionResult<ApiResponse<HealthCheckResponse>>> Ready()
    {
        var databaseReady = await _db.Database.CanConnectAsync();
        if (!databaseReady)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ApiResponse<HealthCheckResponse>
            {
                Success = false,
                Message = "Service is not ready.",
                Data = new HealthCheckResponse
                {
                    Status = "not-ready",
                    Probe = "ready",
                    Checks = new Dictionary<string, string>
                    {
                        ["database"] = "not-ready"
                    }
                }
            });
        }

        return Ok(new ApiResponse<HealthCheckResponse>
        {
            Message = "Service is ready.",
            Data = new HealthCheckResponse
            {
                Status = "ready",
                Probe = "ready",
                Checks = new Dictionary<string, string>
                {
                    ["database"] = "ready"
                }
            }
        });
    }
}
