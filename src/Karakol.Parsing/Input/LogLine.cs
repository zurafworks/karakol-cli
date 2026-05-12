namespace Karakol.Parsing.Input;

public sealed record LogLine(int LineNumber, string Content, long? ByteOffset = null);
