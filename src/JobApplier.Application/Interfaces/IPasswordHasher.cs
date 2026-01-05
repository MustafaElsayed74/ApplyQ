namespace JobApplier.Application.Interfaces;

/// <summary>
/// Password hashing service interface
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a password
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verify password against hash
    /// </summary>
    bool VerifyPassword(string password, string hash);
}
