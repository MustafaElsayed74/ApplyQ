namespace JobApplier.Application.Interfaces;

/// <summary>
/// Interface for CV parsing with OpenAI
/// </summary>
public interface IOpenAICVParsingService
{
    /// <summary>
    /// Parse CV text into structured JSON using OpenAI
    /// </summary>
    /// <param name="extractedText">Raw text extracted from CV</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JSON string with parsed CV structure</returns>
    /// <remarks>
    /// TODO: Configure OpenAI API key via:
    /// - Environment variable: OPENAI_API_KEY
    /// - appsettings.json: OpenAI:ApiKey
    /// 
    /// Expected JSON structure:
    /// {
    ///   "personalInfo": { "name": "", "email": "", "phone": "", "location": "" },
    ///   "summary": "",
    ///   "experiences": [ { "company": "", "position": "", "duration": "", "description": "" } ],
    ///   "education": [ { "school": "", "degree": "", "field": "", "year": "" } ],
    ///   "skills": [ "" ],
    ///   "certifications": [ "" ],
    ///   "languages": [ "" ]
    /// }
    /// </remarks>
    Task<string> ParseCVAsync(string extractedText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if OpenAI service is properly configured
    /// </summary>
    /// <returns>True if API key is set</returns>
    bool IsConfigured();
}
