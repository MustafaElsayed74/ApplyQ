namespace JobApplier.Domain.Entities;

/// <summary>
/// User entity for authentication and profile management
/// </summary>
public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    // TODO: Add subscription tier, usage limits, preferences
    public bool IsActive { get; private set; }

    public User() { }

    public User(string email, string firstName, string lastName)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
    }

    // TODO: Add business methods (ChangePassword, UpdateProfile, etc.)
}
