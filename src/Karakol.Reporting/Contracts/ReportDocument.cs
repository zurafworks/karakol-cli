namespace Karakol.Reporting.Contracts;

public sealed record ReportDocument(
    string SchemaVersion,
    Guid ScanId,
    string ToolName,
    string ToolVersion,
    DateTimeOffset GeneratedAt,
    ReportSourceDocument Source,
    ReportSummaryDocument Summary,
    IReadOnlyCollection<ReportFindingDocument> Findings,
    IReadOnlyCollection<string> Recommendations,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record ReportSourceDocument(string FilePath, string Format, long SizeInBytes);

public sealed record ReportSummaryDocument(
    int TotalLines,
    int ParsedEvents,
    int FailedLines,
    int FindingsCount,
    int SuspiciousEvents,
    int OverallRisk,
    string OverallSeverity,
    IReadOnlyDictionary<string, int> CategoryCounts,
    IReadOnlyDictionary<string, int> SeverityCounts,
    IReadOnlyCollection<ReportTopItemDocument> TopSourceIps,
    IReadOnlyCollection<ReportTopItemDocument> TopTargetUrls,
    IReadOnlyCollection<ReportTopItemDocument> TopUserAgents,
    IReadOnlyCollection<ReportTimelineBucketDocument> Timeline);

public sealed record ReportTopItemDocument(string Value, int Count);

public sealed record ReportTimelineBucketDocument(DateTimeOffset? Timestamp, int EventCount, int FindingCount);

public sealed record ReportFindingDocument(
    Guid Id,
    string Category,
    string Severity,
    int RiskScore,
    string RiskSeverity,
    IReadOnlyCollection<ReportRiskComponentDocument> RiskComponents,
    double Confidence,
    string DetectorId,
    string DetectorName,
    string DetectionType,
    string Title,
    string Description,
    string Reason,
    IReadOnlyCollection<ReportEvidenceDocument> Evidence,
    string RecommendedAction,
    string? SourceIp,
    string? TargetResource,
    IReadOnlyCollection<string> Tags);

public sealed record ReportRiskComponentDocument(string Name, int Contribution, string Explanation);

public sealed record ReportEvidenceDocument(string Field, string? Value, string? MatchedPattern, string Explanation);
