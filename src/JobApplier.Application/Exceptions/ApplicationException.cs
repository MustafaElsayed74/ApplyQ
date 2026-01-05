namespace JobApplier.Application.Exceptions;

/// <summary>
/// Base exception for application layer
/// </summary>
public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message, string? code = null)
        : base(message)
    {
        Code = code ?? GetType().Name;
    }

    public string Code { get; }
}
