using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

using Tubes_POS_API.Data;
using Tubes_POS_API.Options;
using Tubes_POS_API.Models;
using Tubes_POS_API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<ReportService>();

// === Database (SQLite) ===
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=pos.db"));

// === Services (Dependency Injection) ===
builder.Services.AddScoped<ITransactionService, TransactionService>();

var apiOptions = builder.Configuration.GetSection("Api").Get<ApiOptions>() ?? new ApiOptions();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(apiOptions.Version, new OpenApiInfo
    {
        Title = apiOptions.Name,
        Version = apiOptions.Version,
        Description = apiOptions.Description
    });
});

var app = builder.Build();

// Auto-create database & apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/swagger/{apiOptions.Version}/swagger.json", $"{apiOptions.Name} {apiOptions.Version}");
        options.DocumentTitle = apiOptions.Name;
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<Tubes_POS_API.Middleware.ExceptionHandlingMiddleware>();

app.UseStatusCodePages(async statusCodeContext =>
{
    var response = statusCodeContext.HttpContext.Response;

    if (response.StatusCode != StatusCodes.Status404NotFound || response.HasStarted)
    {
        return;
    }

    var requestPath = statusCodeContext.HttpContext.Request.Path.Value ?? string.Empty;
    var payload = new ApiErrorResponse
    {
        Message = "Endpoint tidak ditemukan.",
        StatusCode = StatusCodes.Status404NotFound,
        Errors = [ $"Route {requestPath} tidak tersedia." ]
    };

    response.ContentType = "application/json";
    await response.WriteAsync(JsonSerializer.Serialize(payload));
});

app.UseAuthorization();

app.MapControllers();

app.Run();
