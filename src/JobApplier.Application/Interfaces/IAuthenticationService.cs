namespace JobApplier.Application.Interfaces;

using JobApplier.Application.DTOs.Auth;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Login with email and password
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logout (revoke refresh tokens)
    /// </summary>
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
}
