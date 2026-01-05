using JobApplier.Domain.Entities;

namespace JobApplier.Domain.Entities;

/// <summary>
/// Represents a job description submitted by a user.
/// Can be submitted as plain text or extracted from image via OCR.
/// </summary>
public class JobDescription : Entity
{
    /// <summary>
    /// The user who submitted this job description.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The full text content of the job description.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// The source of the job description: "text" or "image"
    /// </summary>
    public string SourceType { get; private set; }

    /// <summary>
    /// If source is image, the stored image file path.
    /// Null if submitted as plain text.
    /// </summary>
    public string? SourceImagePath { get; private set; }

    /// <summary>
    /// The name/filename of the source image.
    /// Null if submitted as plain text.
    /// </summary>
    public string? SourceImageFileName { get; private set; }

    /// <summary>
    /// File size of the source image in bytes.
    /// Null if submitted as plain text.
    /// </summary>
    public long? SourceImageSizeBytes { get; private set; }

    /// <summary>
    /// Whether the text was extracted from an image via OCR.
    /// False if submitted as plain text.
    /// </summary>
    public bool IsOCRExtracted { get; private set; }

    /// <summary>
    /// User-provided title or reference for this job description.
    /// Helps identify the job position.
    /// </summary>
    public string? JobTitle { get; private set; }

    /// <summary>
    /// Optional company name if user provided it.
    /// </summary>
    public string? CompanyName { get; private set; }

    /// <summary>
    /// Navigation property - User who submitted this job description
    /// </summary>
    public virtual User? User { get; private set; }

    /// <summary>
    /// Navigation property - Cover letters generated for this job description
    /// </summary>
    public virtual ICollection<CoverLetter> CoverLetters { get; private set; } = new List<CoverLetter>();

    private JobDescription() { }

    /// <summary>
    /// Create a plain text job description submission.
    /// </summary>
    public static JobDescription CreateFromText(
        Guid userId,
        string description,
        string? jobTitle = null,
        string? companyName = null)
    {
        return new JobDescription
        {
            UserId = userId,
            Description = description,
            SourceType = "text",
            SourceImagePath = null,
            SourceImageFileName = null,
            SourceImageSizeBytes = null,
            IsOCRExtracted = false,
            JobTitle = jobTitle,
            CompanyName = companyName
        };
    }

    /// <summary>
    /// Create a job description from OCR-extracted text.
    /// </summary>
    public static JobDescription CreateFromOCR(
        Guid userId,
        string extractedText,
        string sourceImagePath,
        string sourceImageFileName,
        long sourceImageSizeBytes,
        string? jobTitle = null,
        string? companyName = null)
    {
        return new JobDescription
        {
            UserId = userId,
            Description = extractedText,
            SourceType = "image",
            SourceImagePath = sourceImagePath,
            SourceImageFileName = sourceImageFileName,
            SourceImageSizeBytes = sourceImageSizeBytes,
            IsOCRExtracted = true,
            JobTitle = jobTitle,
            CompanyName = companyName
        };
    }

    /// <summary>
    /// Update the description text.
    /// </summary>
    public void UpdateDescription(string description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update metadata (job title and company name).
    /// </summary>
    public void UpdateMetadata(string? jobTitle, string? companyName)
    {
        JobTitle = jobTitle;
        CompanyName = companyName;
        UpdatedAt = DateTime.UtcNow;
    }
}
