using Karakol.Shared.Errors;

namespace Karakol.Shared.Results;

public sealed class Result<T>
{
    private readonly List<Warning> _warnings = [];

    private Result(T? value, Error? error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public Error? Error { get; }

    public IReadOnlyCollection<Warning> Warnings => _warnings;

    public static Result<T> Success(T value) => new(value, null, true);

    public static Result<T> Failure(Error error) => new(default, error, false);

    public Result<T> WithWarning(Warning warning)
    {
        _warnings.Add(warning);
        return this;
    }
}
