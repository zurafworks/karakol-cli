namespace Karakol.Reporting.Results;

public sealed record ReportWriteResult(string FilePath, string Format, long SizeInBytes);
