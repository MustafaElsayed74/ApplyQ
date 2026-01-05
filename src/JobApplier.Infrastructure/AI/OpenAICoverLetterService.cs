using JobApplier.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace JobApplier.Infrastructure.AI;

/// <summary>
/// Service for generating professional cover letters using OpenAI API.
/// Implements deterministic prompting to ensure consistent, quality outputs.
/// </summary>
public class OpenAICoverLetterService : ICoverLetterGenerationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAICoverLetterService> _logger;
    private readonly string _apiKey;
    private readonly string _model;

    private int _lastPromptTokens = 0;
    private int _lastCompletionTokens = 0;
    private int _lastTotalTokens = 0;

    public OpenAICoverLetterService(
        IConfiguration configuration,
        ILogger<OpenAICoverLetterService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Read API key from configuration
        _apiKey = _configuration["OpenAI:ApiKey"] ??
                  Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                  string.Empty;

        _model = _configuration["OpenAI:CoverLetterModel"] ?? "gpt-4-turbo";

        if (!IsConfigured())
        {
            _logger.LogWarning(
                "OpenAI API key not configured. Set OpenAI:ApiKey in appsettings.json or OPENAI_API_KEY environment variable.");
        }
    }

    public async Task<string> GenerateCoverLetterAsync(
        string cvParsedJson,
        string jobDescription,
        string? additionalContext = null)
    {
        try
        {
            if (!IsConfigured())
            {
                throw new InvalidOperationException(
                    "OpenAI API is not configured. Please set API key before generating cover letters.");
            }

            // Build the deterministic system prompt
            var systemPrompt = BuildSystemPrompt();

            // Build the user prompt with CV data and job description
            var userPrompt = BuildUserPrompt(cvParsedJson, jobDescription, additionalContext);

            _logger.LogInformation(
                "Generating cover letter using {Model} model",
                _model);

            // TODO: Implement actual OpenAI API call
            // 
            // IMPLEMENTATION NOTES:
            //
            // 1. Install NuGet packages:
            //    dotnet add package OpenAI.Net
            //    or
            //    dotnet add package Betalgo.OpenAI.GPT3
            //
            // 2. Create OpenAI client with API key:
            //    var client = new OpenAIClient(new ApiAuthentication(_apiKey));
            //
            // 3. Send chat completion request:
            //    var request = new ChatCompletionCreateRequest
            //    {
            //        Model = _model,
            //        Messages = new List<ChatMessage>
            //        {
            //            new ChatMessage { Role = ChatRoleEnum.System, Content = systemPrompt },
            //            new ChatMessage { Role = ChatRoleEnum.User, Content = userPrompt }
            //        },
            //        Temperature = 0.7,  // Balance creativity with consistency
            //        MaxTokens = 1000,   // Allow up to ~300 words (250-350 target)
            //        TopP = 0.95         // Nucleus sampling for natural language
            //    };
            //
            //    var response = await client.ChatCompletion.CreateCompletion(request);
            //
            // 4. Extract usage information:
            //    _lastPromptTokens = response.Usage.PromptTokens;
            //    _lastCompletionTokens = response.Usage.CompletionTokens;
            //    _lastTotalTokens = response.Usage.TotalTokens;
            //
            // 5. Extract generated text:
            //    var generatedText = response.Choices.First().Message.Content;
            //    return generatedText.Trim();
            //
            // PROMPT TUNING TODOS:
            // - [ ] Adjust system prompt tone (more formal/casual)
            // - [ ] Fine-tune word count constraints (currently aiming for 250-350)
            // - [ ] Experiment with temperature (0.5-0.9 range)
            // - [ ] Test different max_tokens values (800-1200)
            // - [ ] Add industry-specific variations (tech vs. finance, etc.)
            // - [ ] Test with different CV formats (JSON schema variations)
            // - [ ] Measure quality metrics (relevance, readability, compliance)
            // - [ ] A/B test different prompt variations
            //

            // Placeholder response for now
            _logger.LogWarning(
                "OpenAI cover letter generation not implemented. " +
                "Using placeholder response. See code comments for implementation.");

            // Set usage to placeholder values
            _lastPromptTokens = 500;
            _lastCompletionTokens = 250;
            _lastTotalTokens = 750;

            var placeholderContent = GeneratePlaceholderCoverLetter(cvParsedJson, jobDescription);
            return placeholderContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cover letter from OpenAI API");
            throw;
        }
    }

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(_apiKey);
    }

    public string GetModel()
    {
        return _model;
    }

    public (int promptTokens, int completionTokens, int totalTokens) GetLastUsage()
    {
        return (_lastPromptTokens, _lastCompletionTokens, _lastTotalTokens);
    }

    // ============= Private Helper Methods =============

    /// <summary>
    /// Build the system prompt that defines the cover letter generation behavior.
    /// This prompt is deterministic and doesn't include user-specific data.
    /// </summary>
    private string BuildSystemPrompt()
    {
        return @"You are an expert professional cover letter writer with deep knowledge of recruiting and hiring practices.

Your task is to generate a professional, compelling cover letter that:
1. Is between 250-350 words (mandatory constraint)
2. Matches the candidate's actual qualifications with the job requirements
3. Highlights relevant skills and experiences from the provided CV
4. Shows genuine interest in the specific role and company
5. Maintains a professional but personable tone
6. Avoids generic phrases or clichés
7. Never mentions compensation or benefits (unless job description specifically asks)
8. Never makes up or assumes any information not provided in the CV
9. Uses strong action verbs and specific accomplishments
10. Ends with a clear call to action

Structure: Brief opening (why interested), 2-3 middle paragraphs (relevant skills/experience), strong closing (next steps).

CRITICAL: Do not hallucinate or invent any information. Only use what is explicitly provided in the CV data.";
    }

    /// <summary>
    /// Build the user prompt with CV data and job description.
    /// Structures the prompt to provide clear context for deterministic generation.
    /// </summary>
    private string BuildUserPrompt(string cvParsedJson, string jobDescription, string? additionalContext)
    {
        var prompt = new System.Text.StringBuilder();

        prompt.AppendLine("CANDIDATE CV DATA (Parsed):");
        prompt.AppendLine("=".PadRight(50, '='));
        prompt.AppendLine(cvParsedJson);
        prompt.AppendLine();

        prompt.AppendLine("TARGET JOB DESCRIPTION:");
        prompt.AppendLine("=".PadRight(50, '='));
        prompt.AppendLine(jobDescription);
        prompt.AppendLine();

        if (!string.IsNullOrEmpty(additionalContext))
        {
            prompt.AppendLine("ADDITIONAL PREFERENCES:");
            prompt.AppendLine("=".PadRight(50, '='));
            prompt.AppendLine(additionalContext);
            prompt.AppendLine();
        }

        prompt.AppendLine("INSTRUCTIONS:");
        prompt.AppendLine("Generate a cover letter tailored to this specific position.");
        prompt.AppendLine("Match the candidate's actual experience to the job requirements.");
        prompt.AppendLine("Length: 250-350 words exactly.");
        prompt.AppendLine("Output: Cover letter text only (no additional commentary or metadata).");

        return prompt.ToString();
    }

    /// <summary>
    /// Generate a placeholder cover letter for demonstration.
    /// This should be replaced with actual OpenAI API integration.
    /// </summary>
    private string GeneratePlaceholderCoverLetter(string cvParsedJson, string jobDescription)
    {
        try
        {
            // Try to extract candidate name from parsed CV JSON
            var candidateName = "Valued Candidate";
            try
            {
                var cvDoc = JsonDocument.Parse(cvParsedJson);
                if (cvDoc.RootElement.TryGetProperty("personalInfo", out var personalInfo) &&
                    personalInfo.TryGetProperty("name", out var name))
                {
                    candidateName = name.GetString() ?? "Valued Candidate";
                }
            }
            catch (JsonException)
            {
                _logger.LogWarning("Could not parse CV JSON for placeholder generation");
            }

            // Try to extract job title from job description (first line)
            var jobTitle = "the position";
            var firstLine = jobDescription.Split('\n').FirstOrDefault();
            if (!string.IsNullOrEmpty(firstLine) && firstLine.Length < 100)
            {
                jobTitle = firstLine.Trim();
            }

            return $@"Dear Hiring Manager,

I am writing to express my strong interest in the {jobTitle} opportunity at your esteemed organization.

With a proven track record of professional achievements and a deep commitment to excellence, I am confident that my skills and experiences align perfectly with your team's needs. Throughout my career, I have demonstrated the ability to deliver results, collaborate effectively with diverse teams, and drive meaningful impact in every role I undertake.

Your organization's commitment to innovation and growth particularly resonates with my professional values. I am excited about the prospect of contributing my expertise to your team and helping drive your company's continued success. I am confident that my background, combined with my passion for this field, positions me as an ideal candidate for this role.

I would welcome the opportunity to discuss how my qualifications and enthusiasm can contribute to your team's success. Thank you for considering my application. I look forward to the possibility of speaking with you soon.

Best regards,
{candidateName}

[NOTE: This is a placeholder cover letter generated without OpenAI integration. 
Implement actual OpenAI API call to replace this placeholder with AI-generated content.]";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating placeholder cover letter");
            return "[Error generating cover letter. Please check the implementation.]";
        }
    }
}
