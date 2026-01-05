namespace JobApplier.Application.Services;

using JobApplier.Application.DTOs.CV;
using JobApplier.Application.Interfaces;
using JobApplier.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for CV upload and parsing operations
/// </summary>
public sealed class CVService
{
    private readonly IFileStorageService _fileStorage;
    private readonly ITextExtractionService _textExtraction;
    private readonly ICVRepository _cvRepository;
    private readonly IOpenAICVParsingService _aiParsingService;
    private readonly ILogger<CVService> _logger;

    private static readonly string[] AllowedExtensions = { ".pdf", ".docx" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public CVService(
        IFileStorageService fileStorage,
        ITextExtractionService textExtraction,
        ICVRepository cvRepository,
        IOpenAICVParsingService aiParsingService,
        ILogger<CVService> logger)
    {
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _textExtraction = textExtraction ?? throw new ArgumentNullException(nameof(textExtraction));
        _cvRepository = cvRepository ?? throw new ArgumentNullException(nameof(cvRepository));
        _aiParsingService = aiParsingService ?? throw new ArgumentNullException(nameof(aiParsingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Upload a CV file
    /// </summary>
    public async Task<CVUploadResponse> UploadCVAsync(
        Guid userId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        // Validate file
        ValidateFile(file);

        try
        {
            // Get file extension and checksum
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileChecksum = await CalculateChecksumAsync(file, cancellationToken);

            // Check for duplicate CV
            var existingCV = await _cvRepository.GetByChecksumAsync(userId, fileChecksum, cancellationToken);
            if (existingCV != null)
            {
                _logger.LogWarning("Duplicate CV detected for user {UserId}", userId);
                return new CVUploadResponse
                {
                    CVId = existingCV.Id,
                    FileName = existingCV.FileName,
                    FileType = extension.TrimStart('.'),
                    FileSizeBytes = existingCV.FileSizeBytes,
                    IsParsed = existingCV.IsParsed,
                    CreatedAt = existingCV.CreatedAt,
                    Message = "This CV has already been uploaded."
                };
            }

            // Save file
            var (filePath, _) = await _fileStorage.SaveFileAsync(userId, file, cancellationToken);

            // Extract text based on file type
            var extractedText = extension switch
            {
                ".pdf" => await _textExtraction.ExtractTextFromPdfAsync(filePath, cancellationToken),
                ".docx" => await _textExtraction.ExtractTextFromDocxAsync(filePath, cancellationToken),
                _ => throw new InvalidOperationException($"Unsupported file type: {extension}")
            };

            // Create CV entity
            var cv = new CV(
                userId,
                file.FileName,
                extension.TrimStart('.'),
                filePath,
                file.Length,
                extractedText,
                fileChecksum);

            await _cvRepository.AddAsync(cv, cancellationToken);
            await _cvRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "CV uploaded successfully for user {UserId}: {FileName} ({FileSize} bytes)",
                userId,
                file.FileName,
                file.Length);

            // Start parsing in background (TODO: use background job service)
            _ = Task.Run(() => ParseCVAsync(cv.Id, cancellationToken), cancellationToken);

            return new CVUploadResponse
            {
                CVId = cv.Id,
                FileName = file.FileName,
                FileType = extension.TrimStart('.'),
                FileSizeBytes = file.Length,
                IsParsed = false,
                CreatedAt = cv.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading CV for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get CV details by ID
    /// </summary>
    public async Task<CVDetailsResponse?> GetCVDetailsAsync(
        Guid cvId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cv = await _cvRepository.GetByIdAsync(cvId, cancellationToken);

        if (cv == null || cv.UserId != userId)
        {
            _logger.LogWarning("CV not found or unauthorized access: {CVId} by user {UserId}", cvId, userId);
            return null;
        }

        return new CVDetailsResponse
        {
            CVId = cv.Id,
            FileName = cv.FileName,
            FileType = cv.FileType,
            FileSizeBytes = cv.FileSizeBytes,
            ExtractedText = cv.ExtractedText,
            ParsedDataJson = cv.ParsedDataJson,
            IsParsed = cv.IsParsed,
            ParsedAt = cv.ParsedAt,
            CreatedAt = cv.CreatedAt,
            UpdatedAt = cv.UpdatedAt
        };
    }

    /// <summary>
    /// Get all CVs for a user
    /// </summary>
    public async Task<List<CVDetailsResponse>> GetUserCVsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cvs = await _cvRepository.GetByUserIdAsync(userId, cancellationToken);

        return cvs.Select(cv => new CVDetailsResponse
        {
            CVId = cv.Id,
            FileName = cv.FileName,
            FileType = cv.FileType,
            FileSizeBytes = cv.FileSizeBytes,
            ExtractedText = cv.ExtractedText,
            ParsedDataJson = cv.ParsedDataJson,
            IsParsed = cv.IsParsed,
            ParsedAt = cv.ParsedAt,
            CreatedAt = cv.CreatedAt,
            UpdatedAt = cv.UpdatedAt
        }).ToList();
    }

    /// <summary>
    /// Parse CV text using OpenAI (internal use)
    /// </summary>
    private async Task ParseCVAsync(Guid cvId, CancellationToken cancellationToken)
    {
        try
        {
            var cv = await _cvRepository.GetByIdAsync(cvId, cancellationToken);
            if (cv == null)
            {
                _logger.LogWarning("CV not found for parsing: {CVId}", cvId);
                return;
            }

            if (!_aiParsingService.IsConfigured())
            {
                _logger.LogWarning(
                    "OpenAI service not configured. CV parsing skipped for {CVId}. " +
                    "Configure OPENAI_API_KEY environment variable or OpenAI:ApiKey in appsettings.json",
                    cvId);
                return;
            }

            _logger.LogInformation("Starting CV parsing for {CVId}", cvId);

            var parsedJson = await _aiParsingService.ParseCVAsync(cv.ExtractedText, cancellationToken);
            cv.MarkAsParsed(parsedJson);

            await _cvRepository.UpdateAsync(cv, cancellationToken);
            await _cvRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CV parsing completed for {CVId}", cvId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing CV {CVId}", cvId);
        }
    }

    /// <summary>
    /// Delete a CV
    /// </summary>
    public async Task<bool> DeleteCVAsync(
        Guid cvId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cv = await _cvRepository.GetByIdAsync(cvId, cancellationToken);

        if (cv == null || cv.UserId != userId)
        {
            _logger.LogWarning("CV not found or unauthorized deletion: {CVId} by user {UserId}", cvId, userId);
            return false;
        }

        try
        {
            await _fileStorage.DeleteFileAsync(cv.FilePath, cancellationToken);
            await _cvRepository.DeleteAsync(cvId, cancellationToken);
            await _cvRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CV deleted: {CVId} for user {UserId}", cvId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting CV {CVId}", cvId);
            throw;
        }
    }

    /// <summary>
    /// Validate uploaded file
    /// </summary>
    private static void ValidateFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required and cannot be empty", nameof(file));

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB", nameof(file));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}", nameof(file));
    }

    /// <summary>
    /// Calculate SHA256 checksum of file
    /// </summary>
    private static async Task<string> CalculateChecksumAsync(IFormFile file, CancellationToken cancellationToken)
    {
        using var stream = file.OpenReadStream();
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = await Task.Run(() => sha256.ComputeHash(stream), cancellationToken);
        return Convert.ToHexString(hash);
    }
}
