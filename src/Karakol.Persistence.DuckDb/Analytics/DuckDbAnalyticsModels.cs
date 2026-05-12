namespace Karakol.Persistence.DuckDb.Analytics;

public sealed record SourceIpFindingCount(string SourceIp, int FindingCount);

public sealed record ThreatCategoryTrend(string Category, DateOnly Day, int FindingCount);

public sealed record SeverityTrend(string Severity, DateOnly Day, int FindingCount);

public sealed record ParserFailureRatio(string ScanId, string SourcePath, double Ratio);

public sealed record SensitivePathAccess(string TargetResource, int FindingCount);
