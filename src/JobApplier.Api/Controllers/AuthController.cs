namespace JobApplier.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JobApplier.Application.DTOs.Auth;
using JobApplier.Application.Interfaces;
using JobApplier.Application.Exceptions;

/// <summary>
/// Authentication controller for user registration, login, and token refresh
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication tokens and user info</returns>
    /// <response code="200">User registered successfully</response>
    /// <response code="400">Invalid registration details</response>
    /// <response code="409">Email already registered</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RegisterAsync(request, cancellationToken);
            _logger.LogInformation("User registered: {Email}", request.Email);
            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);

            // Return 409 Conflict for duplicate email
            if (ex.Message.Contains("already registered"))
                return Conflict(new ErrorResponse { Message = ex.Message });

            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse { Message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication tokens and user info</returns>
    /// <response code="200">Login successful</response>
    /// <response code="400">Invalid credentials</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            _logger.LogInformation("User logged in: {Email}", request.Email);
            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Login failed: {Message}", ex.Message);
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse { Message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New authentication tokens</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid refresh token</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
            _logger.LogInformation("Token refreshed for user: {UserId}", response.User.Id);
            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh error");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse { Message = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Logout user (revoke refresh tokens)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Logout successful</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _authService.LogoutAsync(userId.Value, cancellationToken);
            _logger.LogInformation("User logged out: {UserId}", userId);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout error");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse { Message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Get current user ID from JWT claims
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
            return userId;

        return null;
    }
}

/// <summary>
/// Error response DTO
/// </summary>
public sealed class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}
