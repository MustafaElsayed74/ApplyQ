namespace JobApplier.Domain.Entities;

/// <summary>
/// Refresh token entity for token rotation and revocation
/// </summary>
public class RefreshToken : Entity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public RefreshToken() { }

    public RefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Check if token is valid (not expired and not revoked)
    /// </summary>
    public bool IsValid() => !IsRevoked && !IsExpired;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Revoke the refresh token
    /// </summary>
    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
