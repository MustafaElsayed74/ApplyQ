using JobApplier.Application.DTOs;
using JobApplier.Application.Interfaces;
using JobApplier.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace JobApplier.Application.Services;

/// <summary>
/// Service for orchestrating cover letter generation workflow.
/// Handles CV and job description retrieval, generation, and storage.
/// </summary>
public class CoverLetterService
{
    private readonly ICoverLetterRepository _repository;
    private readonly ICoverLetterGenerationService _generationService;
    private readonly ICVRepository _cvRepository;
    private readonly IJobDescriptionRepository _jobDescriptionRepository;
    private readonly ILogger<CoverLetterService> _logger;

    public CoverLetterService(
        ICoverLetterRepository repository,
        ICoverLetterGenerationService generationService,
        ICVRepository cvRepository,
        IJobDescriptionRepository jobDescriptionRepository,
        ILogger<CoverLetterService> logger)
    {
        _repository = repository;
        _generationService = generationService;
        _cvRepository = cvRepository;
        _jobDescriptionRepository = jobDescriptionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Generate a cover letter from a parsed CV and job description.
    /// </summary>
    public async Task<CoverLetterResponse> GenerateCoverLetterAsync(
        Guid userId,
        GenerateCoverLetterRequest request)
    {
        try
        {
            // Retrieve and validate CV
            var cv = await _cvRepository.GetByIdAsync(request.CVId);
            if (cv == null)
            {
                throw new KeyNotFoundException("CV not found");
            }

            if (cv.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to use this CV");
            }

            if (!cv.IsParsed || string.IsNullOrEmpty(cv.ParsedDataJson))
            {
                throw new InvalidOperationException(
                    "CV must be parsed and have structured data before generating cover letters. " +
                    "Please wait for CV parsing to complete or check that your CV was properly processed.");
            }

            // Retrieve and validate job description
            var jobDescription = await _jobDescriptionRepository.GetByIdAsync(request.JobDescriptionId);
            if (jobDescription == null)
            {
                throw new KeyNotFoundException("Job description not found");
            }

            if (jobDescription.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to use this job description");
            }

            // Check if cover letter already exists for this combination
            var existingCoverLetter = await _repository.GetByCVAndJobDescriptionAsync(
                request.CVId,
                request.JobDescriptionId);

            if (existingCoverLetter != null)
            {
                _logger.LogInformation(
                    "Cover letter already exists for CV {CVId} and job description {JobDescriptionId}",
                    request.CVId,
                    request.JobDescriptionId);
                return MapToResponse(existingCoverLetter, "Cover letter already exists for this combination");
            }

            // Check if service is configured
            if (!_generationService.IsConfigured())
            {
                throw new InvalidOperationException(
                    "Cover letter generation service is not configured. " +
                    "Please set OpenAI API key in configuration.");
            }

            _logger.LogInformation(
                "Generating cover letter for user {UserId}, CV {CVId}, Job Description {JobDescriptionId}",
                userId,
                request.CVId,
                request.JobDescriptionId);

            // Generate cover letter using OpenAI
            var generatedContent = await _generationService.GenerateCoverLetterAsync(
                cv.ParsedDataJson,
                jobDescription.Description,
                request.AdditionalContext);

            if (string.IsNullOrWhiteSpace(generatedContent))
            {
                throw new InvalidOperationException("Cover letter generation returned empty content");
            }

            // Calculate word count
            var wordCount = CountWords(generatedContent);

            // Validate word count (should be 250-350)
            if (wordCount < 200 || wordCount > 400)
            {
                _logger.LogWarning(
                    "Generated cover letter has {WordCount} words (expected 250-350). " +
                    "Storing anyway but flagging for review.",
                    wordCount);
            }

            // Get token usage
            var (promptTokens, completionTokens, totalTokens) = _generationService.GetLastUsage();

            // Create cover letter entity
            var coverLetter = CoverLetter.CreateFromGeneration(
                userId,
                request.CVId,
                request.JobDescriptionId,
                generatedContent,
                wordCount,
                totalTokens,
                _generationService.GetModel());

            // Store in database
            await _repository.AddAsync(coverLetter);
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Cover letter {CoverLetterId} generated successfully for user {UserId} " +
                "({WordCount} words, {TokensUsed} tokens)",
                coverLetter.Id,
                userId,
                wordCount,
                totalTokens);

            return MapToResponse(coverLetter, "Cover letter generated successfully");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cover letter for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get a specific cover letter.
    /// </summary>
    public async Task<CoverLetterResponse> GetCoverLetterAsync(Guid coverLetterId, Guid userId)
    {
        try
        {
            var coverLetter = await _repository.GetByIdAsync(coverLetterId);

            if (coverLetter == null)
            {
                throw new KeyNotFoundException("Cover letter not found");
            }

            if (coverLetter.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to access this cover letter");
            }

            return MapToResponse(coverLetter, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cover letter {CoverLetterId}", coverLetterId);
            throw;
        }
    }

    /// <summary>
    /// Get all cover letters for a user.
    /// </summary>
    public async Task<List<CoverLetterResponse>> GetUserCoverLettersAsync(Guid userId)
    {
        try
        {
            var coverLetters = await _repository.GetByUserIdAsync(userId);
            return coverLetters.Select(cl => MapToResponse(cl, "")).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cover letters for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get cover letters for a specific CV.
    /// </summary>
    public async Task<List<CoverLetterResponse>> GetCVCoverLettersAsync(Guid cvId, Guid userId)
    {
        try
        {
            var cv = await _cvRepository.GetByIdAsync(cvId);
            if (cv == null)
            {
                throw new KeyNotFoundException("CV not found");
            }

            if (cv.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to access this CV");
            }

            var coverLetters = await _repository.GetByCVIdAsync(cvId);
            return coverLetters.Select(cl => MapToResponse(cl, "")).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cover letters for CV {CVId}", cvId);
            throw;
        }
    }

    /// <summary>
    /// Delete a cover letter.
    /// </summary>
    public async Task DeleteCoverLetterAsync(Guid coverLetterId, Guid userId)
    {
        try
        {
            var coverLetter = await _repository.GetByIdAsync(coverLetterId);

            if (coverLetter == null)
            {
                throw new KeyNotFoundException("Cover letter not found");
            }

            if (coverLetter.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this cover letter");
            }

            await _repository.DeleteAsync(coverLetter);
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Cover letter {CoverLetterId} deleted successfully",
                coverLetterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cover letter {CoverLetterId}", coverLetterId);
            throw;
        }
    }

    /// <summary>
    /// Update cover letter notes (for manual editing or version tracking).
    /// </summary>
    public async Task<CoverLetterResponse> AddNotesAsync(Guid coverLetterId, Guid userId, string notes)
    {
        try
        {
            var coverLetter = await _repository.GetByIdAsync(coverLetterId);

            if (coverLetter == null)
            {
                throw new KeyNotFoundException("Cover letter not found");
            }

            if (coverLetter.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this cover letter");
            }

            coverLetter.AddNotes(notes);
            await _repository.UpdateAsync(coverLetter);
            await _repository.SaveChangesAsync();

            return MapToResponse(coverLetter, "Notes added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding notes to cover letter {CoverLetterId}", coverLetterId);
            throw;
        }
    }

    /// <summary>
    /// Update cover letter content (for user manual edits).
    /// </summary>
    public async Task<CoverLetterResponse> UpdateContentAsync(
        Guid coverLetterId,
        Guid userId,
        string newContent)
    {
        try
        {
            var coverLetter = await _repository.GetByIdAsync(coverLetterId);

            if (coverLetter == null)
            {
                throw new KeyNotFoundException("Cover letter not found");
            }

            if (coverLetter.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this cover letter");
            }

            var wordCount = CountWords(newContent);
            coverLetter.UpdateContent(newContent, wordCount);

            await _repository.UpdateAsync(coverLetter);
            await _repository.SaveChangesAsync();

            _logger.LogInformation(
                "Cover letter {CoverLetterId} content updated",
                coverLetterId);

            return MapToResponse(coverLetter, "Content updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cover letter {CoverLetterId}", coverLetterId);
            throw;
        }
    }

    // ============= Private Helper Methods =============

    /// <summary>
    /// Count words in text (simple word splitting).
    /// </summary>
    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Map CoverLetter entity to response DTO.
    /// </summary>
    private CoverLetterResponse MapToResponse(CoverLetter coverLetter, string message)
    {
        return new CoverLetterResponse
        {
            CoverLetterId = coverLetter.Id,
            CVId = coverLetter.CVId,
            JobDescriptionId = coverLetter.JobDescriptionId,
            GeneratedContent = coverLetter.GeneratedContent,
            WordCount = coverLetter.WordCount,
            TokensUsed = coverLetter.TokensUsed,
            Model = coverLetter.Model,
            Notes = coverLetter.Notes,
            CreatedAt = coverLetter.CreatedAt,
            UpdatedAt = coverLetter.UpdatedAt,
            Message = message
        };
    }
}
