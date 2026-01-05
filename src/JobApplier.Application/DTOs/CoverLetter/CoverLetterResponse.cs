namespace JobApplier.Application.DTOs;

/// <summary>
/// Response containing a generated or retrieved cover letter.
/// </summary>
public class CoverLetterResponse
{
    /// <summary>
    /// Unique identifier for the cover letter.
    /// </summary>
    public Guid CoverLetterId { get; set; }

    /// <summary>
    /// The CV ID this cover letter was generated from.
    /// </summary>
    public Guid CVId { get; set; }

    /// <summary>
    /// The job description ID this cover letter targets.
    /// </summary>
    public Guid JobDescriptionId { get; set; }

    /// <summary>
    /// The final generated cover letter text.
    /// </summary>
    public string GeneratedContent { get; set; } = string.Empty;

    /// <summary>
    /// Word count of the generated content (typically 250-350).
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// OpenAI API tokens consumed for this generation.
    /// </summary>
    public int TokensUsed { get; set; }

    /// <summary>
    /// The OpenAI model used (e.g., "gpt-4-turbo").
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Custom notes about this generation.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// When the cover letter was generated.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the cover letter was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Status message from the generation.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
