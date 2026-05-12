using Karakol.Domain.Events;

namespace Karakol.Parsing.Results;

public sealed record ParseResult(bool IsSuccess, SecurityEvent? Event, ParseError? Error, int LineNumber)
{
    public static ParseResult Success(SecurityEvent securityEvent) => new(true, securityEvent, null, securityEvent.LineNumber);

    public static ParseResult Failure(int lineNumber, string rawLine, string reason) => new(false, null, new ParseError(lineNumber, rawLine, reason), lineNumber);
}
