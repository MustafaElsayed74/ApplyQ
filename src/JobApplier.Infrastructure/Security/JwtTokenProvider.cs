namespace JobApplier.Infrastructure.Security;

using JobApplier.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// JWT token provider implementation
/// </summary>
public sealed class JwtTokenProvider : IJwtTokenProvider
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenProvider(
        string secretKey,
        string issuer,
        string audience,
        int expirationMinutes = 15)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new ArgumentException("Secret key cannot be empty", nameof(secretKey));

        if (secretKey.Length < 32)
            throw new ArgumentException("Secret key must be at least 32 characters", nameof(secretKey));

        _secretKey = secretKey;
        _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        _audience = audience ?? throw new ArgumentNullException(nameof(audience));
        _expirationMinutes = expirationMinutes;
    }

    /// <summary>
    /// Generate JWT access token
    /// </summary>
    public string GenerateAccessToken(Guid userId, string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("sub", userId.ToString()), // Standard claim for user ID
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID for uniqueness
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate refresh token (cryptographically secure random)
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Get access token expiration in seconds
    /// </summary>
    public int GetAccessTokenExpirationSeconds() => _expirationMinutes * 60;
}
