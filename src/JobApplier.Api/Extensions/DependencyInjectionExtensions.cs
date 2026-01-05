namespace JobApplier.Api.Extensions;

using JobApplier.Application.Extensions;
using JobApplier.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JWT Authentication
        services.AddJwtAuthentication(configuration);

        // Swagger
        services.AddSwaggerDocumentation();

        // CORS
        // TODO: Configure CORS from environment settings (allowed origins)
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Register Application services
        services.AddApplication();

        // Register Infrastructure services
        services.AddInfrastructure(configuration);

        return services;
    }
}
