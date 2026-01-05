namespace JobApplier.Application.Validators.Auth;

using JobApplier.Application.DTOs.Auth;

/// <summary>
/// Validation logic for authentication requests
/// </summary>
public static class AuthenticationValidator
{
    /// <summary>
    /// Validate register request
    /// </summary>
    public static string? ValidateRegister(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return "Email is required";

        if (request.Email.Length > 254)
            return "Email is too long";

        if (!IsValidEmail(request.Email))
            return "Email format is invalid";

        if (string.IsNullOrWhiteSpace(request.FirstName))
            return "First name is required";

        if (request.FirstName.Length > 100)
            return "First name is too long";

        if (string.IsNullOrWhiteSpace(request.LastName))
            return "Last name is required";

        if (request.LastName.Length > 100)
            return "Last name is too long";

        if (string.IsNullOrWhiteSpace(request.Password))
            return "Password is required";

        var passwordError = ValidatePassword(request.Password);
        if (passwordError != null)
            return passwordError;

        if (request.Password != request.ConfirmPassword)
            return "Passwords do not match";

        return null;
    }

    /// <summary>
    /// Validate login request
    /// </summary>
    public static string? ValidateLogin(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return "Email is required";

        if (string.IsNullOrWhiteSpace(request.Password))
            return "Password is required";

        return null;
    }

    /// <summary>
    /// Validate password strength
    /// </summary>
    public static string? ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Password is required";

        if (password.Length < 8)
            return "Password must be at least 8 characters";

        if (password.Length > 256)
            return "Password is too long";

        if (!password.Any(c => char.IsUpper(c)))
            return "Password must contain at least one uppercase letter";

        if (!password.Any(c => char.IsLower(c)))
            return "Password must contain at least one lowercase letter";

        if (!password.Any(char.IsDigit))
            return "Password must contain at least one digit";

        if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)))
            return "Password must contain at least one special character";

        return null;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
