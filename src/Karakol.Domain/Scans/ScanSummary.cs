using Karakol.Domain.Enums;
using Karakol.Domain.Risk;
using Karakol.Domain.ValueObjects;

namespace Karakol.Domain.Scans;

public sealed record ScanSummary(
    int TotalLines,
    int ParsedEvents,
    int FailedLines,
    int FindingsCount,
    int SuspiciousEvents,
    RiskScore OverallRisk,
    IReadOnlyDictionary<ThreatCategory, int> CategoryCounts,
    IReadOnlyDictionary<Severity, int> SeverityCounts,
    IReadOnlyCollection<TopSourceIpSummary> TopSourceIps,
    IReadOnlyCollection<TopTargetUrlSummary> TopTargetUrls,
    IReadOnlyCollection<TopUserAgentSummary> TopUserAgents,
    IReadOnlyCollection<TimelineBucket> Timeline,
    IReadOnlyCollection<string> Recommendations);
