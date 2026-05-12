namespace Karakol.Shared.Errors;

public sealed record Error(string Code, string Message, string? Details = null, ErrorType Type = ErrorType.Unexpected);
