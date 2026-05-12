namespace Karakol.Parsing.Results;

public sealed record ParseError(int LineNumber, string RawLine, string Reason);
