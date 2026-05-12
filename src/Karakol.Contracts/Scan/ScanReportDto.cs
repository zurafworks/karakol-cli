using Karakol.Contracts.Reports;

namespace Karakol.Contracts.Scan;

public sealed record ScanReportDto(
    Guid ScanId,
    string Source,
    string DetectedFormat,
    int TotalLines,
    int ParsedEvents,
    int FailedLines,
    int FindingsCount,
    int RiskScore,
    string RiskSeverity,
    IReadOnlyCollection<ReportFileDto> Reports);
