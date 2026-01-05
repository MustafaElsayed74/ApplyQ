using JobApplier.Application.DTOs;
using JobApplier.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplier.Api.Controllers;

/// <summary>
/// API endpoints for job description submission and management.
/// </summary>
[ApiController]
[Route("api/job-descriptions")]
[Authorize]
public class JobDescriptionsController : ControllerBase
{
    private readonly JobDescriptionService _jobDescriptionService;
    private readonly ILogger<JobDescriptionsController> _logger;

    public JobDescriptionsController(
        JobDescriptionService jobDescriptionService,
        ILogger<JobDescriptionsController> logger)
    {
        _jobDescriptionService = jobDescriptionService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a new job description.
    /// Can be submitted as plain text or as an image (PNG/JPG) with OCR extraction.
    /// </summary>
    /// <param name="request">Job description submission request</param>
    /// <returns>Job description response with metadata</returns>
    /// <response code="200">Job description submitted successfully</response>
    /// <response code="400">Invalid request (missing text/image or file validation failed)</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Server error</response>
    [HttpPost("submit")]
    [ProducesResponseType(typeof(JobDescriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitJobDescription([FromForm] JobDescriptionSubmitRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var response = await _jobDescriptionService.SubmitJobDescriptionAsync(userId, request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in job description submission: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting job description");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while submitting the job description" });
        }
    }

    /// <summary>
    /// Get all job descriptions for the authenticated user.
    /// </summary>
    /// <returns>List of job descriptions</returns>
    /// <response code="200">List of job descriptions</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<JobDescriptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobDescriptions()
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var responses = await _jobDescriptionService.GetUserJobDescriptionsAsync(userId);
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job descriptions");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving job descriptions" });
        }
    }

    /// <summary>
    /// Get a specific job description by ID.
    /// </summary>
    /// <param name="jobDescriptionId">ID of the job description</param>
    /// <returns>Job description details</returns>
    /// <response code="200">Job description found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Job description not found or unauthorized access</response>
    /// <response code="500">Server error</response>
    [HttpGet("{jobDescriptionId:guid}")]
    [ProducesResponseType(typeof(JobDescriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobDescription(Guid jobDescriptionId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var response = await _jobDescriptionService.GetJobDescriptionAsync(jobDescriptionId, userId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Job description not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to job description: {Message}", ex.Message);
            return NotFound(new { message = "Job description not found or unauthorized access" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job description {JobDescriptionId}", jobDescriptionId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the job description" });
        }
    }

    /// <summary>
    /// Update the description text of a job description.
    /// </summary>
    /// <param name="jobDescriptionId">ID of the job description</param>
    /// <param name="request">Update request containing new description text</param>
    /// <returns>Updated job description</returns>
    /// <response code="200">Job description updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Job description not found</response>
    /// <response code="500">Server error</response>
    [HttpPut("{jobDescriptionId:guid}")]
    [ProducesResponseType(typeof(JobDescriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateJobDescription(
        Guid jobDescriptionId,
        [FromBody] UpdateDescriptionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { message = "Description cannot be empty" });
            }

            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var response = await _jobDescriptionService.UpdateDescriptionAsync(
                jobDescriptionId,
                userId,
                request.Description);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Job description not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to job description: {Message}", ex.Message);
            return NotFound(new { message = "Job description not found or unauthorized access" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job description {JobDescriptionId}", jobDescriptionId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the job description" });
        }
    }

    /// <summary>
    /// Update metadata (job title and company name) for a job description.
    /// </summary>
    /// <param name="jobDescriptionId">ID of the job description</param>
    /// <param name="request">Update request containing job title and company name</param>
    /// <returns>Updated job description</returns>
    /// <response code="200">Metadata updated successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Job description not found</response>
    /// <response code="500">Server error</response>
    [HttpPatch("{jobDescriptionId:guid}/metadata")]
    [ProducesResponseType(typeof(JobDescriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMetadata(
        Guid jobDescriptionId,
        [FromBody] UpdateMetadataRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var response = await _jobDescriptionService.UpdateMetadataAsync(
                jobDescriptionId,
                userId,
                request.JobTitle,
                request.CompanyName);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Job description not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to job description: {Message}", ex.Message);
            return NotFound(new { message = "Job description not found or unauthorized access" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating metadata for job description {JobDescriptionId}", jobDescriptionId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating metadata" });
        }
    }

    /// <summary>
    /// Delete a job description (and associated image file if applicable).
    /// </summary>
    /// <param name="jobDescriptionId">ID of the job description</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Job description deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Job description not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{jobDescriptionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteJobDescription(Guid jobDescriptionId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            await _jobDescriptionService.DeleteJobDescriptionAsync(jobDescriptionId, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Job description not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized deletion attempt of job description: {Message}", ex.Message);
            return NotFound(new { message = "Job description not found or unauthorized access" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job description {JobDescriptionId}", jobDescriptionId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the job description" });
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
/// Request to update job description text.
/// </summary>
public class UpdateDescriptionRequest
{
    /// <summary>
    /// The new description text.
    /// </summary>
    public string Description { get; set; }
}

/// <summary>
/// Request to update job description metadata.
/// </summary>
public class UpdateMetadataRequest
{
    /// <summary>
    /// Optional job title.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Optional company name.
    /// </summary>
    public string? CompanyName { get; set; }
}
