using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tubes POS API",
        Version = "v1",
        Description = "Backend API untuk sistem POS Food & Beverage."
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tubes POS API v1");
        options.DocumentTitle = "Tubes POS API";
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<Tubes_POS_API.Middleware.ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
