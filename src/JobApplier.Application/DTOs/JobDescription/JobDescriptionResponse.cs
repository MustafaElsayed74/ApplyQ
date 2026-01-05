namespace JobApplier.Application.DTOs;

/// <summary>
/// Response for job description submission/retrieval.
/// </summary>
public class JobDescriptionResponse
{
    /// <summary>
    /// Unique identifier for the job description.
    /// </summary>
    public Guid JobDescriptionId { get; set; }

    /// <summary>
    /// The full text content of the job description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Source type: "text" or "image"
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// Whether the text was extracted from image via OCR.
    /// </summary>
    public bool IsOCRExtracted { get; set; }

    /// <summary>
    /// Optional job title.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Optional company name.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// File size of source image in bytes, if applicable.
    /// </summary>
    public long? SourceImageSizeBytes { get; set; }

    /// <summary>
    /// When the job description was submitted.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the job description was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Status message from the submission.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
