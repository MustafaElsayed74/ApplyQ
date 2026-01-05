namespace JobApplier.Application.Interfaces;

using JobApplier.Domain.Entities;

/// <summary>
/// Repository interface for RefreshToken entity
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Get refresh token by token value
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add refresh token
    /// </summary>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update refresh token
    /// </summary>
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke all refresh tokens for user
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
