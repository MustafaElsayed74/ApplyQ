namespace JobApplier.Application.Exceptions;

/// <summary>
/// Exception thrown for authentication-related errors
/// </summary>
public sealed class AuthenticationException : ApplicationException
{
    public AuthenticationException(string message)
        : base(message, "AuthenticationError")
    {
    }
}
