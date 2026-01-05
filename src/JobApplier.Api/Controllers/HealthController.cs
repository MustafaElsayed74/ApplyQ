namespace JobApplier.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Health check endpoint
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Check API health status
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            // TODO: Add database connectivity check
            // TODO: Add external service checks (OpenAI, OCR provider, etc.)
        });
    }
}
