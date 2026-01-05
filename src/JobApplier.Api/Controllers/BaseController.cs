namespace JobApplier.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Base controller for all API endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current user ID from claims
    /// </summary>
    /// <remarks>TODO: Implement claim parsing from JWT token</remarks>
    protected string? GetCurrentUserId()
    {
        return User.FindFirst("sub")?.Value ??
               User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    }
}
