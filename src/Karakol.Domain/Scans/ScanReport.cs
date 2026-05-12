using Karakol.Domain.Findings;
using Karakol.Domain.Sources;
using Karakol.Domain.ValueObjects;

namespace Karakol.Domain.Scans;

public sealed record ScanReport(
    ScanId ScanId,
    string ToolName,
    string ToolVersion,
    DateTimeOffset GeneratedAt,
    LogSource Source,
    ScanSummary Summary,
    IReadOnlyCollection<DetectionFinding> Findings,
    IReadOnlyCollection<TimelineBucket> Timeline,
    IReadOnlyCollection<string> Recommendations,
    ScanConfigurationSnapshot Configuration,
    IReadOnlyDictionary<string, string> Metadata);
