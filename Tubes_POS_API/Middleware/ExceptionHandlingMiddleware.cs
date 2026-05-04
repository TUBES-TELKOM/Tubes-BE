using System.Net;
using System.Text.Json;
using Tubes_POS_API.Models;

namespace Tubes_POS_API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        var response = new ApiErrorResponse
        {
            Message = message,
            StatusCode = statusCode,
            Errors = _environment.IsDevelopment()
                ? [exception.Message]
                : []
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
