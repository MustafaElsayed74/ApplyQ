using Microsoft.AspNetCore.Http;

namespace JobApplier.Application.DTOs;

/// <summary>
/// Request to submit a job description.
/// Can be submitted as plain text OR as an image file (PNG/JPG).
/// </summary>
public class JobDescriptionSubmitRequest
{
    /// <summary>
    /// The job description as plain text.
    /// Required if ImageFile is not provided.
    /// </summary>
    public string? DescriptionText { get; set; }

    /// <summary>
    /// Image file containing job description (PNG or JPG).
    /// Required if DescriptionText is not provided.
    /// Maximum 5 MB.
    /// </summary>
    public IFormFile? ImageFile { get; set; }

    /// <summary>
    /// Optional job title or position name.
    /// Helps identify which job this description is for.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Optional company name.
    /// </summary>
    public string? CompanyName { get; set; }
}
