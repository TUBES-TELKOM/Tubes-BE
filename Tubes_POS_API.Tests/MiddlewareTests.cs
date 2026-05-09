using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Hosting;
using Tubes_POS_API.Middleware;
using Tubes_POS_API.Models;

namespace Tubes_POS_API.Tests;

public class MiddlewareTests
{
    [Fact]
    public async Task ExceptionMiddleware_ShouldReturnJsonConflictForInvalidOperation()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("payment conflict"),
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            new TestHostEnvironment());

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiErrorResponse>(json)!;

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Equal("payment conflict", response.Message);
        Assert.Contains("payment conflict", response.Errors);
    }

    [Fact]
    public async Task NotFoundMiddleware_ShouldWriteJson404Response()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        context.Response.Body = new MemoryStream();

        var middleware = new NotFoundResponseMiddleware(_ =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiErrorResponse>(json)!;

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal("Endpoint tidak ditemukan.", response.Message);
        Assert.Contains("/test", response.Errors.First());
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public string EnvironmentName { get; set; } = Environments.Development;
    }
}
