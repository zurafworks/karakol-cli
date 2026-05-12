using Karakol.Domain.Enums;
using Karakol.Domain.Scans;

namespace Karakol.Persistence.Abstractions;

public sealed record ScanHistoryItem(ScanId ScanId, string SourcePath, DateTimeOffset GeneratedAt, int RiskScore, Severity Severity);
