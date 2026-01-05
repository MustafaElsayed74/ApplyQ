namespace JobApplier.Application.DTOs.CV;

/// <summary>
/// Response DTO for CV upload
/// </summary>
public class CVUploadResponse
{
    /// <summary>
    /// Unique identifier for the uploaded CV
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
    /// Whether CV has been parsed
    /// </summary>
    public bool IsParsed { get; set; }

    /// <summary>
    /// When the CV was uploaded
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    public string Message { get; set; } = "CV uploaded successfully. Parsing in progress...";
}
