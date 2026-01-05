namespace JobApplier.Application.Extensions;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Application layer dependency injection extension
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // TODO: Register application services
        // services.AddScoped<IResumeService, ResumeService>();
        // services.AddScoped<ICoverLetterService, CoverLetterService>();
        // services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

        return services;
    }
}
