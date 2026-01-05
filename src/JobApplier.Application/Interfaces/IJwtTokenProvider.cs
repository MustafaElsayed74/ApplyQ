namespace JobApplier.Application.Interfaces;

/// <summary>
/// JWT token provider interface
/// </summary>
public interface IJwtTokenProvider
{
    /// <summary>
    /// Generate JWT access token
    /// </summary>
    string GenerateAccessToken(Guid userId, string email);

    /// <summary>
    /// Generate refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Get access token expiration time in seconds
    /// </summary>
    int GetAccessTokenExpirationSeconds();
}
