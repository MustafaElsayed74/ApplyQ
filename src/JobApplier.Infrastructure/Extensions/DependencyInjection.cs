namespace JobApplier.Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using JobApplier.Application.Interfaces;
using JobApplier.Infrastructure.Persistence;
using JobApplier.Infrastructure.Persistence.Repositories;
using JobApplier.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Infrastructure layer dependency injection extension
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.Parse("8.0.0-mysql")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Security Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // JWT Token Provider
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"] ?? "JobApplier";
        var audience = jwtSettings["Audience"] ?? "JobApplierClient";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "15");

        services.AddSingleton<IJwtTokenProvider>(new JwtTokenProvider(secretKey, issuer, audience, expirationMinutes));

        return services;
    }
}
