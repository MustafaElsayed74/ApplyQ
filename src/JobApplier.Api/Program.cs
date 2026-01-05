using JobApplier.Api.Extensions;
using JobApplier.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// TODO: Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    // TODO: Add file sink, cloud logging (Application Insights) based on environment
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

// Use middleware
app.UseSwaggerDocumentation();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.MapControllers();

// TODO: Add health check endpoint
// app.MapHealthChecks("/health");

try
{
    Log.Information("Starting JobApplier API");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
