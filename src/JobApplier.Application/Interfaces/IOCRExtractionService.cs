namespace JobApplier.Application.Interfaces;

/// <summary>
/// Service for extracting text from images using OCR.
/// </summary>
public interface IOCRExtractionService
{
    /// <summary>
    /// Extract text from a PNG or JPG image file.
    /// </summary>
    /// <param name="filePath">Path to the image file</param>
    /// <returns>Extracted text from the image</returns>
    Task<string> ExtractTextFromImageAsync(string filePath);

    /// <summary>
    /// Get the MIME type for a given file extension.
    /// </summary>
    string GetContentType(string extension);

    /// <summary>
    /// Check if OCR service is configured and available.
    /// </summary>
    bool IsConfigured();
}
