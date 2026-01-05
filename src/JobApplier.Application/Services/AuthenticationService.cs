namespace JobApplier.Application.Services;

using JobApplier.Application.DTOs.Auth;
using JobApplier.Application.Exceptions;
using JobApplier.Application.Interfaces;
using JobApplier.Application.Validators.Auth;
using JobApplier.Domain.Entities;

/// <summary>
/// Authentication service implementation
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenProvider _tokenProvider;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenProvider tokenProvider)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenProvider = tokenProvider;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request
        var validationError = AuthenticationValidator.ValidateRegister(request);
        if (validationError != null)
            throw new AuthenticationException(validationError);

        // Check if email already exists
        var emailExists = await _userRepository.EmailExistsAsync(request.Email, cancellationToken);
        if (emailExists)
            throw new AuthenticationException("Email already registered");

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create user
        var user = new User(request.Email, request.FirstName, request.LastName, passwordHash);

        // Save user
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _tokenProvider.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _tokenProvider.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken(
            user.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(7)); // 7 days expiration

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _tokenProvider.GetAccessTokenExpirationSeconds(),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Validate request
        var validationError = AuthenticationValidator.ValidateLogin(request);
        if (validationError != null)
            throw new AuthenticationException(validationError);

        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new AuthenticationException("Invalid email or password");

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new AuthenticationException("Invalid email or password");

        // Check if user is active
        if (!user.IsActive)
            throw new AuthenticationException("User account is inactive");

        // Generate tokens
        var accessToken = _tokenProvider.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _tokenProvider.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken(
            user.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(7)); // 7 days expiration

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _tokenProvider.GetAccessTokenExpirationSeconds(),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new AuthenticationException("Refresh token is required");

        // Find refresh token
        var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken)
            ?? throw new AuthenticationException("Invalid refresh token");

        // Verify token is valid
        if (!tokenEntity.IsValid())
        {
            if (tokenEntity.IsExpired)
                throw new AuthenticationException("Refresh token has expired");
            else
                throw new AuthenticationException("Refresh token has been revoked");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(tokenEntity.UserId, cancellationToken)
            ?? throw new AuthenticationException("User not found");

        // Check if user is active
        if (!user.IsActive)
            throw new AuthenticationException("User account is inactive");

        // Generate new tokens
        var newAccessToken = _tokenProvider.GenerateAccessToken(user.Id, user.Email);
        var newRefreshToken = _tokenProvider.GenerateRefreshToken();

        // Revoke old token
        tokenEntity.Revoke();
        await _refreshTokenRepository.UpdateAsync(tokenEntity, cancellationToken);

        // Save new refresh token
        var newTokenEntity = new RefreshToken(
            user.Id,
            newRefreshToken,
            DateTime.UtcNow.AddDays(7)); // 7 days expiration

        await _refreshTokenRepository.AddAsync(newTokenEntity, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = _tokenProvider.GetAccessTokenExpirationSeconds(),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Revoke all user's refresh tokens
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }
}
