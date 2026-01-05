namespace JobApplier.Application.DTOs.Auth;

/// <summary>
/// Refresh token request DTO
/// </summary>
public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
