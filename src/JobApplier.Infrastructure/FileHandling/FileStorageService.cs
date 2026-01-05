namespace JobApplier.Infrastructure.FileHandling;

using JobApplier.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// File storage service for secure file management
/// </summary>
public sealed class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _storagePath;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Create storage directory: {AppData}/JobApplier/CVs/{UserId}/
        _storagePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "JobApplier",
            "CVs");

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
            _logger.LogInformation("Created CV storage directory: {StoragePath}", _storagePath);
        }
    }

    /// <summary>
    /// Save an uploaded file securely
    /// </summary>
    public async Task<(string filePath, string checksum)> SaveFileAsync(
        Guid userId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required", nameof(file));

        try
        {
            // Create user directory
            var userDirectory = Path.Combine(_storagePath, userId.ToString());
            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }

            // Generate secure filename (timestamp + random guid + original extension)
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var randomName = Guid.NewGuid().ToString("N").Substring(0, 8);
            var extension = Path.GetExtension(file.FileName);
            var secureFileName = $"{timestamp}_{randomName}{extension}";

            var filePath = Path.Combine(userDirectory, secureFileName);

            // Save file
            await using var stream = file.OpenReadStream();
            await using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream, cancellationToken);

            _logger.LogInformation(
                "File saved successfully: {FilePath} ({FileSize} bytes)",
                filePath,
                file.Length);

            // Calculate checksum for deduplication
            stream.Seek(0, SeekOrigin.Begin);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = await Task.Run(() => sha256.ComputeHash(stream), cancellationToken);
            var checksum = Convert.ToHexString(hash);

            return (filePath, checksum);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Delete a stored file
    /// </summary>
    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath), cancellationToken);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Check if a file exists
    /// </summary>
    public async Task<bool> FileExistsAsync(string filePath)
    {
        return await Task.Run(() => File.Exists(filePath));
    }
}
