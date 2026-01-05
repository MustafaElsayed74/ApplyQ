namespace JobApplier.Domain.ValueObjects;

/// <summary>
/// Email value object - immutable email representation
/// </summary>
public sealed class Email : IEquatable<Email>
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Email>.Failure("Email cannot be empty");

        var trimmedEmail = value.Trim().ToLowerInvariant();

        if (trimmedEmail.Length > 254)
            return Result<Email>.Failure("Email is too long");

        if (!IsValidEmail(trimmedEmail))
            return Result<Email>.Failure("Email format is invalid");

        return Result<Email>.Success(new Email(trimmedEmail));
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

    public override bool Equals(object? obj) => Equals(obj as Email);
    public bool Equals(Email? other) => other?.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}
