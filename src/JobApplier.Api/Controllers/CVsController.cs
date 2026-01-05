namespace JobApplier.Api.Controllers;

using JobApplier.Application.DTOs.CV;
using JobApplier.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Controller for CV upload and management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CVsController : ControllerBase
{
    private readonly CVService _cvService;
    private readonly ILogger<CVsController> _logger;

    public CVsController(CVService cvService, ILogger<CVsController> logger)
    {
        _cvService = cvService ?? throw new ArgumentNullException(nameof(cvService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Upload a new CV
    /// </summary>
    /// <remarks>
    /// Accepts PDF or DOCX files up to 10 MB.
    /// Extracts text and queues for OpenAI parsing.
    /// </remarks>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(CVUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadCV(
        [FromForm] CVUploadRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();

            var response = await _cvService.UploadCVAsync(userId, request.File!, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "CV upload validation failed");
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading CV for user {UserId}", GetUserIdFromClaims());
            return StatusCode(500, new ErrorResponse { Message = "An error occurred while uploading the CV." });
        }
    }

    /// <summary>
    /// Get CV details by ID
    /// </summary>
    /// <remarks>
    /// Returns extracted text and parsed CV data if available.
    /// Users can only access their own CVs.
    /// </remarks>
    [HttpGet("{cvId}")]
    [ProducesResponseType(typeof(CVDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCVDetails(
        Guid cvId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();

            var response = await _cvService.GetCVDetailsAsync(cvId, userId, cancellationToken);

            if (response == null)
            {
                _logger.LogWarning("CV not found or unauthorized access: {CVId} by user {UserId}", cvId, userId);
                return NotFound();
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CV {CVId}", cvId);
            return StatusCode(500, new ErrorResponse { Message = "An error occurred while retrieving the CV." });
        }
    }

    /// <summary>
    /// Get all CVs for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CVDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserCVs(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();

            var response = await _cvService.GetUserCVsAsync(userId, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CVs for user {UserId}", GetUserIdFromClaims());
            return StatusCode(500, new ErrorResponse { Message = "An error occurred while retrieving your CVs." });
        }
    }

    /// <summary>
    /// Delete a CV
    /// </summary>
    /// <remarks>
    /// Deletes both the file and database record.
    /// Users can only delete their own CVs.
    /// </remarks>
    [HttpDelete("{cvId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCV(
        Guid cvId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();

            var success = await _cvService.DeleteCVAsync(cvId, userId, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("CV not found or unauthorized deletion: {CVId} by user {UserId}", cvId, userId);
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting CV {CVId}", cvId);
            return StatusCode(500, new ErrorResponse { Message = "An error occurred while deleting the CV." });
        }
    }

    /// <summary>
    /// Extract user ID from JWT claims
    /// </summary>
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("User ID not found in claims");
    }
}
