namespace JobApplier.Application.DTOs.CV;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Request DTO for CV upload
/// </summary>
public class CVUploadRequest
{
    /// <summary>
    /// The CV file (multipart/form-data)
    /// </summary>
    public IFormFile? File { get; set; }
}
