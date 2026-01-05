namespace JobApplier.Application.Interfaces;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Interface for file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Save an uploaded file securely
    /// </summary>
    /// <param name="userId">User ID for organizing storage</param>
    /// <param name="file">The file to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Secure file path and checksum</returns>
    Task<(string filePath, string checksum)> SaveFileAsync(
        Guid userId,
        IFormFile file,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a stored file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a file exists
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>True if file exists</returns>
    Task<bool> FileExistsAsync(string filePath);
}
