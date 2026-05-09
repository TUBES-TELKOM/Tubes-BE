using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Tubes_POS_API.Data;
using Tubes_POS_API.Models;
using Tubes_POS_API.Options;
using Tubes_POS_API.Repositories;
using Tubes_POS_API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<PaymentStateMachine>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<ReportService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=pos.db"));

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

app.UseMiddleware<Tubes_POS_API.Middleware.NotFoundResponseMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
