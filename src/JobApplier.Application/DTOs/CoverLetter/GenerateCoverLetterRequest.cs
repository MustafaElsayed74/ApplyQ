namespace JobApplier.Application.DTOs;

/// <summary>
/// Request to generate a cover letter from CV and job description.
/// </summary>
public class GenerateCoverLetterRequest
{
    /// <summary>
    /// ID of the CV to use for generation.
    /// The CV must be parsed and have ParsedDataJson populated.
    /// </summary>
    public Guid CVId { get; set; }

    /// <summary>
    /// ID of the job description to target with the cover letter.
    /// </summary>
    public Guid JobDescriptionId { get; set; }

    /// <summary>
    /// Optional additional context or requirements for the cover letter.
    /// e.g., "Emphasize leadership experience" or "Highlight remote work capabilities"
    /// </summary>
    public string? AdditionalContext { get; set; }
}
