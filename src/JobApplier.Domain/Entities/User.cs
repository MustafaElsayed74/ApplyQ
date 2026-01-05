namespace JobApplier.Domain.Entities;

/// <summary>
/// User entity - authentication and profile management
/// </summary>
public class User : Entity
{
    private string _email = string.Empty;

    public string Email
    {
        get => _email;
        private set => _email = value.ToLowerInvariant();
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime? EmailConfirmedAt { get; private set; }

    // Navigation properties - only for EF Core
    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public virtual ICollection<CV> CVs { get; private set; } = new List<CV>();
    public virtual ICollection<JobDescription> JobDescriptions { get; private set; } = new List<JobDescription>();
    public virtual ICollection<CoverLetter> CoverLetters { get; private set; } = new List<CoverLetter>();

    public User() { }

    public User(string email, string firstName, string lastName, string passwordHash)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        IsActive = true;
    }

    /// <summary>
    /// Confirm user email
    /// </summary>
    public void ConfirmEmail()
    {
        EmailConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate user account
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update password hash
    /// </summary>
    public void UpdatePasswordHash(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}
