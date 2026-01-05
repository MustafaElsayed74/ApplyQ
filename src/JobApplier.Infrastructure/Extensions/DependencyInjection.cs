namespace JobApplier.Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Infrastructure layer dependency injection extension
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        // TODO: Register DbContext
        // services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // TODO: Register repositories
        // services.AddScoped<IUserRepository, UserRepository>();
        // services.AddScoped<IResumeRepository, ResumeRepository>();

        // TODO: Register external service clients
        // services.AddScoped<IOpenAiService, OpenAiService>();
        // services.AddScoped<IOcrService, OcrService>();
        // services.AddScoped<IFileStorageService, FileStorageService>();

        // TODO: Register Unit of Work pattern if applicable
        // services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
