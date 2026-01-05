namespace JobApplier.Domain.Entities;

/// <summary>
/// CV entity for storing uploaded CVs and parsed data
/// </summary>
public class CV : Entity
{
    /// <summary>
    /// User who owns this CV
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Original file name as uploaded
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// File type (pdf, docx)
    /// </summary>
    public string FileType { get; private set; } = string.Empty;

    /// <summary>
    /// Secure path where the original file is stored
    /// </summary>
    public string FilePath { get; private set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>
    /// Raw text extracted from the CV file
    /// </summary>
    public string ExtractedText { get; private set; } = string.Empty;

    /// <summary>
    /// Parsed CV as JSON (result from OpenAI structuring)
    /// </summary>
    public string ParsedDataJson { get; private set; } = string.Empty;

    /// <summary>
    /// Whether the CV has been successfully parsed by OpenAI
    /// </summary>
    public bool IsParsed { get; private set; }

    /// <summary>
    /// When parsing was completed (null if pending)
    /// </summary>
    public DateTime? ParsedAt { get; private set; }

    /// <summary>
    /// Checksum of the file for deduplication
    /// </summary>
    public string FileChecksum { get; private set; } = string.Empty;

    /// <summary>
    /// Navigation property - User who owns this CV
    /// </summary>
    public virtual User? User { get; private set; }

    /// <summary>
    /// Navigation property - Cover letters generated from this CV
    /// </summary>
    public virtual ICollection<CoverLetter> CoverLetters { get; private set; } = new List<CoverLetter>();

    public CV() { }

    public CV(
        Guid userId,
        string fileName,
        string fileType,
        string filePath,
        long fileSizeBytes,
        string extractedText,
        string fileChecksum)
    {
        UserId = userId;
        FileName = fileName;
        FileType = fileType;
        FilePath = filePath;
        FileSizeBytes = fileSizeBytes;
        ExtractedText = extractedText;
        FileChecksum = fileChecksum;
        IsParsed = false;
        ParsedAt = null;
    }

    /// <summary>
    /// Mark CV as parsed with structured data from OpenAI
    /// </summary>
    public void MarkAsParsed(string parsedDataJson)
    {
        ParsedDataJson = parsedDataJson;
        IsParsed = true;
        ParsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update extracted text (in case re-extraction is needed)
    /// </summary>
    public void UpdateExtractedText(string text)
    {
        ExtractedText = text;
        IsParsed = false;
        ParsedAt = null;
        ParsedDataJson = string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
}
