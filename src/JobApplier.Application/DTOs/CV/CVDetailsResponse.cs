namespace JobApplier.Application.DTOs.CV;

/// <summary>
/// Response DTO for CV details
/// </summary>
public class CVDetailsResponse
{
    /// <summary>
    /// Unique identifier for the CV
    /// </summary>
    public Guid CVId { get; set; }

    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File type (pdf, docx)
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Raw extracted text from the CV
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;

    /// <summary>
    /// Parsed CV data as JSON
    /// </summary>
    public string ParsedDataJson { get; set; } = string.Empty;

    /// <summary>
    /// Whether CV has been successfully parsed
    /// </summary>
    public bool IsParsed { get; set; }

    /// <summary>
    /// When parsing was completed
    /// </summary>
    public DateTime? ParsedAt { get; set; }

    /// <summary>
    /// When the CV was uploaded
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the CV was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
