namespace JobApplier.Application.Extensions;

using Microsoft.Extensions.DependencyInjection;
using JobApplier.Application.Services;
using JobApplier.Application.Interfaces;

/// <summary>
/// Application layer dependency injection extension
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<CVService>();

        return services;
    }
}
