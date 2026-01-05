namespace JobApplier.Application.Interfaces;

/// <summary>
/// Interface for extracting text from documents
/// </summary>
public interface ITextExtractionService
{
    /// <summary>
    /// Extract text from a PDF file
    /// </summary>
    /// <param name="filePath">Path to the PDF file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text</returns>
    Task<string> ExtractTextFromPdfAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract text from a DOCX file
    /// </summary>
    /// <param name="filePath">Path to the DOCX file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text</returns>
    Task<string> ExtractTextFromDocxAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the content type for a file extension
    /// </summary>
    /// <param name="extension">File extension (e.g., ".pdf")</param>
    /// <returns>Content type</returns>
    string GetContentType(string extension);
}
