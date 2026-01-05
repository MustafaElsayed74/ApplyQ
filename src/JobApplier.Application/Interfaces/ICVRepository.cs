namespace JobApplier.Application.Interfaces;

/// <summary>
/// Interface for CV repository operations
/// </summary>
public interface ICVRepository
{
    /// <summary>
    /// Get a CV by ID
    /// </summary>
    /// <param name="cvId">CV ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CV entity or null if not found</returns>
    Task<Domain.Entities.CV?> GetByIdAsync(Guid cvId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all CVs for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of CVs</returns>
    Task<List<Domain.Entities.CV>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get CV by file checksum (for deduplication)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="checksum">File checksum</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CV entity or null if not found</returns>
    Task<Domain.Entities.CV?> GetByChecksumAsync(
        Guid userId,
        string checksum,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new CV
    /// </summary>
    /// <param name="cv">CV entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(Domain.Entities.CV cv, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing CV
    /// </summary>
    /// <param name="cv">CV entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(Domain.Entities.CV cv, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a CV
    /// </summary>
    /// <param name="cvId">CV ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(Guid cvId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
