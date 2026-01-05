namespace JobApplier.Application.Interfaces;

using JobApplier.Domain.Entities;

/// <summary>
/// Repository interface for User entity
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get user by email
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if email exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add user
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
