namespace JobApplier.Application.Interfaces;

/// <summary>
/// Interface for AI cover letter generation service.
/// Handles the OpenAI API integration for generating professional cover letters.
/// </summary>
public interface ICoverLetterGenerationService
{
    /// <summary>
    /// Generate a professional cover letter from parsed CV data and job description.
    /// </summary>
    /// <param name="cvParsedJson">Parsed CV JSON with structured candidate data</param>
    /// <param name="jobDescription">Raw job description text</param>
    /// <param name="additionalContext">Optional additional requirements or preferences</param>
    /// <returns>Generated cover letter text (250-350 words)</returns>
    Task<string> GenerateCoverLetterAsync(
        string cvParsedJson,
        string jobDescription,
        string? additionalContext = null);

    /// <summary>
    /// Check if the cover letter generation service is properly configured.
    /// </summary>
    bool IsConfigured();

    /// <summary>
    /// Get the model name being used for generation.
    /// </summary>
    string GetModel();

    /// <summary>
    /// Get usage information from the last generation (for testing/debugging).
    /// </summary>
    (int promptTokens, int completionTokens, int totalTokens) GetLastUsage();
}
