namespace JobApplier.Infrastructure.Security;

using JobApplier.Application.Interfaces;

/// <summary>
/// Password hashing using PBKDF2 with salt
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // OWASP recommendation

    /// <summary>
    /// Hash a password with PBKDF2
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            // Generate random salt
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            // Hash password with salt
            using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                System.Security.Cryptography.HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Combine salt + hash
                byte[] hashWithSalt = new byte[SaltSize + HashSize];
                System.Buffer.BlockCopy(salt, 0, hashWithSalt, 0, SaltSize);
                System.Buffer.BlockCopy(hash, 0, hashWithSalt, SaltSize, HashSize);

                // Return as base64
                return Convert.ToBase64String(hashWithSalt);
            }
        }
    }

    /// <summary>
    /// Verify password against hash
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            // Decode hash
            byte[] hashWithSalt = Convert.FromBase64String(hash);

            // Extract salt
            byte[] salt = new byte[SaltSize];
            System.Buffer.BlockCopy(hashWithSalt, 0, salt, 0, SaltSize);

            // Hash provided password with extracted salt
            using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                System.Security.Cryptography.HashAlgorithmName.SHA256))
            {
                byte[] hash2 = pbkdf2.GetBytes(HashSize);

                // Compare hashes (constant-time comparison)
                return ByteArraysEqual(hashWithSalt, SaltSize, hash2, 0, HashSize);
            }
        }
        catch
        {
            return false;
        }
    }

    private static bool ByteArraysEqual(byte[] a, int aOffset, byte[] b, int bOffset, int length)
    {
        int result = 0;
        for (int i = 0; i < length; i++)
        {
            result |= a[aOffset + i] ^ b[bOffset + i];
        }
        return result == 0;
    }
}
