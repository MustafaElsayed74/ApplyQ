using JobApplier.Application.DTOs;
using JobApplier.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplier.Api.Controllers;

/// <summary>
/// API endpoints for AI-powered cover letter generation and management.
/// </summary>
[ApiController]
[Route("api/cover-letters")]
[Authorize]
public class CoverLettersController : ControllerBase
{
    private readonly CoverLetterService _coverLetterService;
    private readonly ILogger<CoverLettersController> _logger;

    public CoverLettersController(
        CoverLetterService coverLetterService,
        ILogger<CoverLettersController> logger)
    {
        _coverLetterService = coverLetterService;
        _logger = logger;
    }

    /// <summary>
    /// Generate a professional cover letter from a CV and job description.
    /// </summary>
    /// <param name="request">Request with CV ID, job description ID, and optional context</param>
    /// <returns>Generated cover letter with metadata</returns>
    /// <response code="200">Cover letter generated successfully</response>
    /// <response code="400">Invalid request (missing IDs, CV not parsed, etc.)</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">CV or job description not found</response>
    /// <response code="500">Server error or OpenAI API failure</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(CoverLetterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateCoverLetter([FromBody] GenerateCoverLetterRequest request)
    {
        try
        {
            if (request == null || request.CVId == Guid.Empty || request.JobDescriptionId == Guid.Empty)
            {
                return BadRequest(new { message = "CVId and JobDescriptionId are required" });
            }

            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var response = await _coverLetterService.GenerateCoverLetterAsync(userId, request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
            return NotFound(new { message = "Resource not found or unauthorized access" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cover letter");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while generating the cover letter" });
        }
    }

    /// <summary>
    /// Get all cover letters for the authenticated user.
    /// </summary>
    /// <returns>List of cover letters</returns>
    /// <response code="200">List of cover letters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CoverLetterResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCoverLetters()
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var responses = await _coverLetterService.GetUserCoverLettersAsync(userId);
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cover letters");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving cover letters" });
        }
    }

    /// <summary>
    /// Get a specific cover letter by ID.
    /// </summary>
    /// <param name="coverLetterId">ID of the cover letter</param>
    /// <returns>Cover letter details</returns>
    /// <response code="200">Cover letter found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Cover letter not found or unauthorized access</response>
    /// <response code="500">Server error</response>
    [HttpGet("{coverLetterId:guid}")]
    [ProducesResponseType(typeof(CoverLetterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCoverLetter(Guid coverLetterId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var response = await _coverLetterService.GetCoverLetterAsync(coverLetterId, userId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Cover letter not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to cover letter: {Message}", ex.Message);
            return NotFound(new { message = "Cover letter not found or unauthorized access" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cover letter {CoverLetterId}", coverLetterId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the cover letter" });
        }
    }

    /// <summary>
    /// Get all cover letters generated from a specific CV.
    /// </summary>
    /// <param name="cvId">ID of the CV</param>
    /// <returns>List of cover letters for this CV</returns>
    /// <response code="200">List of cover letters</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">CV not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("cv/{cvId:guid}")]
    [ProducesResponseType(typeof(List<CoverLetterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCVCoverLetters(Guid cvId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var responses = await _coverLetterService.GetCVCoverLettersAsync(cvId, userId);
            return Ok(responses);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("CV not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to CV: {Message}", ex.Message);
            return NotFound(new { message = "CV not found or unauthorized access" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cover letters for CV {CVId}", cvId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving cover letters" });
        }
    }

    /// <summary>
    /// Update the content of a cover letter (for user manual edits).
    /// </summary>
    /// <param name="coverLetterId">ID of the cover letter</param>
    /// <param name="request">Update request with new content</param>
    /// <returns>Updated cover letter</returns>
    /// <response code="200">Cover letter updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Cover letter not found</response>
    /// <response code="500">Server error</response>
    [HttpPut("{coverLetterId:guid}")]
    [ProducesResponseType(typeof(CoverLetterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCoverLetterContent(Guid coverLetterId, [FromBody] UpdateCoverLetterRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.GeneratedContent))
            {
                return BadRequest(new { message = "GeneratedContent cannot be empty" });
            }

            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var response = await _coverLetterService.UpdateContentAsync(coverLetterId, userId, request.GeneratedContent);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Cover letter not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to cover letter: {Message}", ex.Message);
            return NotFound(new { message = "Cover letter not found or unauthorized access" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cover letter {CoverLetterId}", coverLetterId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the cover letter" });
        }
    }

    /// <summary>
    /// Add or update notes on a cover letter.
    /// </summary>
    /// <param name="coverLetterId">ID of the cover letter</param>
    /// <param name="request">Request with notes</param>
    /// <returns>Updated cover letter</returns>
    /// <response code="200">Notes added successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Cover letter not found</response>
    /// <response code="500">Server error</response>
    [HttpPatch("{coverLetterId:guid}/notes")]
    [ProducesResponseType(typeof(CoverLetterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNotes(Guid coverLetterId, [FromBody] AddNotesRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var response = await _coverLetterService.AddNotesAsync(coverLetterId, userId, request.Notes);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Cover letter not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to cover letter: {Message}", ex.Message);
            return NotFound(new { message = "Cover letter not found or unauthorized access" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding notes to cover letter {CoverLetterId}", coverLetterId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while adding notes" });
        }
    }

    /// <summary>
    /// Delete a cover letter.
    /// </summary>
    /// <param name="coverLetterId">ID of the cover letter</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Cover letter deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Cover letter not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{coverLetterId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCoverLetter(Guid coverLetterId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            await _coverLetterService.DeleteCoverLetterAsync(coverLetterId, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Cover letter not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized deletion attempt of cover letter: {Message}", ex.Message);
            return NotFound(new { message = "Cover letter not found or unauthorized access" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cover letter {CoverLetterId}", coverLetterId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the cover letter" });
        }
    }

    // ============= Private Helper Methods =============

    /// <summary>
    /// Extract user ID from JWT claims.
    /// </summary>
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Guid.Empty;
        }

        return userId;
    }
}

/// <summary>
/// Request to update cover letter content.
/// </summary>
public class UpdateCoverLetterRequest
{
    /// <summary>
    /// The new cover letter text.
    /// </summary>
    public string GeneratedContent { get; set; } = string.Empty;
}

/// <summary>
/// Request to add notes to a cover letter.
/// </summary>
public class AddNotesRequest
{
    /// <summary>
    /// Notes or metadata about the cover letter.
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
