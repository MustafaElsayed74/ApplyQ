using JobApplier.Application.DTOs;
using JobApplier.Application.Interfaces;
using JobApplier.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JobApplier.Application.Services;

/// <summary>
/// Service for managing job description submissions.
/// Handles both plain text and image (OCR) submissions.
/// </summary>
public class JobDescriptionService
{
    private readonly IJobDescriptionRepository _repository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IOCRExtractionService _ocrService;
    private readonly ILogger<JobDescriptionService> _logger;

    private const long MaxImageFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedImageExtensions = { ".png", ".jpg", ".jpeg" };

    public JobDescriptionService(
        IJobDescriptionRepository repository,
        IFileStorageService fileStorageService,
        IOCRExtractionService ocrService,
        ILogger<JobDescriptionService> logger)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
        _ocrService = ocrService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a new job description (plain text or image).
    /// </summary>
    public async Task<JobDescriptionResponse> SubmitJobDescriptionAsync(
        Guid userId,
        JobDescriptionSubmitRequest request)
    {
        try
        {
            // Validate that we have either text or image
            if (string.IsNullOrWhiteSpace(request.DescriptionText) && request.ImageFile == null)
            {
                throw new ArgumentException(
                    "Either DescriptionText or ImageFile must be provided");
            }

            JobDescription jobDescription;

            // Handle text submission
            if (!string.IsNullOrWhiteSpace(request.DescriptionText))
            {
                var normalizedText = NormalizeText(request.DescriptionText);

                jobDescription = JobDescription.CreateFromText(
                    userId,
                    normalizedText,
                    request.JobTitle,
                    request.CompanyName);

                _logger.LogInformation(
                    "Submitting job description from plain text for user {UserId}",
                    userId);
            }
            // Handle image submission with OCR
            else
            {
                jobDescription = await SubmitFromImageAsync(
                    userId,
                    request.ImageFile!,
                    request.JobTitle,
                    request.CompanyName);
            }

            // Save to database
            await _repository.AddAsync(jobDescription);
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Job description {JobDescriptionId} submitted successfully for user {UserId}",
                jobDescription.Id,
                userId);

            return MapToResponse(jobDescription, "Job description submitted successfully");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error submitting job description: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting job description for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get a specific job description by ID.
    /// </summary>
    public async Task<JobDescriptionResponse> GetJobDescriptionAsync(
        Guid jobDescriptionId,
        Guid userId)
    {
        try
        {
            var jobDescription = await _repository.GetByIdAsync(jobDescriptionId);

            if (jobDescription == null)
            {
                _logger.LogWarning(
                    "Job description {JobDescriptionId} not found for user {UserId}",
                    jobDescriptionId,
                    userId);
                throw new KeyNotFoundException("Job description not found");
            }

            // Verify ownership
            if (jobDescription.UserId != userId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt to job description {JobDescriptionId} by user {UserId}",
                    jobDescriptionId,
                    userId);
                throw new UnauthorizedAccessException("You are not authorized to access this job description");
            }

            return MapToResponse(jobDescription, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job description {JobDescriptionId}", jobDescriptionId);
            throw;
        }
    }

    /// <summary>
    /// Get all job descriptions for a user.
    /// </summary>
    public async Task<List<JobDescriptionResponse>> GetUserJobDescriptionsAsync(Guid userId)
    {
        try
        {
            var jobDescriptions = await _repository.GetByUserIdAsync(userId);
            return jobDescriptions.Select(jd => MapToResponse(jd, "")).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job descriptions for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Delete a job description (and associated image file if applicable).
    /// </summary>
    public async Task DeleteJobDescriptionAsync(Guid jobDescriptionId, Guid userId)
    {
        try
        {
            var jobDescription = await _repository.GetByIdAsync(jobDescriptionId);

            if (jobDescription == null)
            {
                _logger.LogWarning(
                    "Job description {JobDescriptionId} not found for deletion",
                    jobDescriptionId);
                throw new KeyNotFoundException("Job description not found");
            }

            // Verify ownership
            if (jobDescription.UserId != userId)
            {
                _logger.LogWarning(
                    "Unauthorized deletion attempt of job description {JobDescriptionId} by user {UserId}",
                    jobDescriptionId,
                    userId);
                throw new UnauthorizedAccessException("You are not authorized to delete this job description");
            }

            // Delete associated image file if it exists
            if (!string.IsNullOrEmpty(jobDescription.SourceImagePath))
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(jobDescription.SourceImagePath);
                    _logger.LogInformation(
                        "Deleted image file for job description {JobDescriptionId}",
                        jobDescriptionId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Error deleting image file for job description {JobDescriptionId}",
                        jobDescriptionId);
                    // Don't fail the deletion if file cleanup fails
                }
            }

            // Delete from database
            await _repository.DeleteAsync(jobDescription);
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Job description {JobDescriptionId} deleted successfully",
                jobDescriptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job description {JobDescriptionId}", jobDescriptionId);
            throw;
        }
    }

    /// <summary>
    /// Update the description content.
    /// </summary>
    public async Task<JobDescriptionResponse> UpdateDescriptionAsync(
        Guid jobDescriptionId,
        Guid userId,
        string newDescription)
    {
        try
        {
            var jobDescription = await _repository.GetByIdAsync(jobDescriptionId);

            if (jobDescription == null)
            {
                throw new KeyNotFoundException("Job description not found");
            }

            if (jobDescription.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this job description");
            }

            var normalizedText = NormalizeText(newDescription);
            jobDescription.UpdateDescription(normalizedText);

            await _repository.UpdateAsync(jobDescription);
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Job description {JobDescriptionId} updated successfully",
                jobDescriptionId);

            return MapToResponse(jobDescription, "Job description updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job description {JobDescriptionId}", jobDescriptionId);
            throw;
        }
    }

    /// <summary>
    /// Update metadata (job title and company name).
    /// </summary>
    public async Task<JobDescriptionResponse> UpdateMetadataAsync(
        Guid jobDescriptionId,
        Guid userId,
        string? jobTitle,
        string? companyName)
    {
        try
        {
            var jobDescription = await _repository.GetByIdAsync(jobDescriptionId);

            if (jobDescription == null)
            {
                throw new KeyNotFoundException("Job description not found");
            }

            if (jobDescription.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this job description");
            }

            jobDescription.UpdateMetadata(jobTitle, companyName);

            await _repository.UpdateAsync(jobDescription);
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Job description {JobDescriptionId} metadata updated successfully",
                jobDescriptionId);

            return MapToResponse(jobDescription, "Metadata updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating metadata for job description {JobDescriptionId}", jobDescriptionId);
            throw;
        }
    }

    // ============= Private Helper Methods =============

    /// <summary>
    /// Handle image submission with OCR extraction.
    /// </summary>
    private async Task<JobDescription> SubmitFromImageAsync(
        Guid userId,
        IFormFile imageFile,
        string? jobTitle,
        string? companyName)
    {
        try
        {
            // Validate file
            ValidateImageFile(imageFile);

            _logger.LogInformation(
                "Processing image submission for user {UserId}, filename: {FileName}",
                userId,
                imageFile.FileName);

            // Save the image file
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
            var (savedFilePath, _) = await _fileStorageService.SaveFileAsync(
                userId,
                imageFile);

            _logger.LogInformation(
                "Image file saved at {FilePath}",
                savedFilePath);

            // Extract text using OCR
            string extractedText;
            if (_ocrService.IsConfigured())
            {
                try
                {
                    extractedText = await _ocrService.ExtractTextFromImageAsync(savedFilePath);
                    _logger.LogInformation(
                        "OCR extraction completed for job description image, extracted {CharCount} characters",
                        extractedText.Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OCR extraction failed for image at {FilePath}", savedFilePath);
                    extractedText = "[OCR extraction failed. Please provide text manually.]";
                }
            }
            else
            {
                _logger.LogWarning(
                    "OCR service not configured. Image saved but text extraction skipped.");
                extractedText = "[OCR not configured. Please extract text manually or reconfigure OCR service.]";
            }

            // Normalize extracted text
            var normalizedText = NormalizeText(extractedText);

            // Create job description entity
            var jobDescription = JobDescription.CreateFromOCR(
                userId,
                normalizedText,
                savedFilePath,
                imageFile.FileName,
                imageFile.Length,
                jobTitle,
                companyName);

            return jobDescription;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error with image submission: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image submission for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Validate image file.
    /// </summary>
    private void ValidateImageFile(IFormFile file)
    {
        if (file == null)
        {
            throw new ArgumentException("Image file is required");
        }

        if (file.Length == 0)
        {
            throw new ArgumentException("Image file cannot be empty");
        }

        if (file.Length > MaxImageFileSizeBytes)
        {
            throw new ArgumentException(
                $"Image file size exceeds maximum allowed size of {MaxImageFileSizeBytes / (1024 * 1024)} MB");
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedImageExtensions.Contains(fileExtension))
        {
            throw new ArgumentException(
                "File type not supported. Only PNG and JPG/JPEG files are allowed");
        }
    }

    /// <summary>
    /// Normalize job description text.
    /// </summary>
    private string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Trim whitespace
        text = text.Trim();

        // Normalize line endings to \n
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // Remove excessive blank lines (more than 2 consecutive)
        while (text.Contains("\n\n\n"))
        {
            text = text.Replace("\n\n\n", "\n\n");
        }

        return text;
    }

    /// <summary>
    /// Map JobDescription entity to response DTO.
    /// </summary>
    private JobDescriptionResponse MapToResponse(JobDescription jobDescription, string message)
    {
        return new JobDescriptionResponse
        {
            JobDescriptionId = jobDescription.Id,
            Description = jobDescription.Description,
            SourceType = jobDescription.SourceType,
            IsOCRExtracted = jobDescription.IsOCRExtracted,
            JobTitle = jobDescription.JobTitle,
            CompanyName = jobDescription.CompanyName,
            SourceImageSizeBytes = jobDescription.SourceImageSizeBytes,
            CreatedAt = jobDescription.CreatedAt,
            UpdatedAt = jobDescription.UpdatedAt,
            Message = message
        };
    }
}
