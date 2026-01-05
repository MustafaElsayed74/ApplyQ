namespace JobApplier.Domain.ValueObjects;

/// <summary>
/// Result monad for error handling without exceptions
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);

    public void Match(
        Action<T> onSuccess,
        Action<string> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value!);
        else
            onFailure(Error!);
    }
}
