using System.Text.Json;
using Tubes_POS_API.Models;

namespace Tubes_POS_API.Middleware;

public sealed class NotFoundResponseMiddleware
{
    private readonly RequestDelegate _next;

    public NotFoundResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.HasStarted || context.Response.StatusCode != StatusCodes.Status404NotFound)
        {
            return;
        }

        var requestPath = context.Request.Path.Value ?? string.Empty;
        var payload = new ApiErrorResponse
        {
            Message = "Endpoint tidak ditemukan.",
            StatusCode = StatusCodes.Status404NotFound,
            Errors = [$"Route {requestPath} tidak tersedia."]
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
