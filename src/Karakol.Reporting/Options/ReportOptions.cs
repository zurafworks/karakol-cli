namespace Karakol.Reporting.Options;

public sealed record ReportOptions(string OutputDirectory, bool MaskSensitiveData, bool IncludeRawSamples);
