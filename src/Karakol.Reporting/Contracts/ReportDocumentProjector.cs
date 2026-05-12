using Karakol.Domain.Scans;

namespace Karakol.Reporting.Contracts;

public static class ReportDocumentProjector
{
    public static ReportDocument Project(ScanReport report)
    {
        return new ReportDocument(
            "karakol.report.v1",
            report.ScanId.Value,
            report.ToolName,
            report.ToolVersion,
            report.GeneratedAt,
            new ReportSourceDocument(report.Source.FilePath, report.Source.Format.ToString(), report.Source.SizeInBytes),
            new ReportSummaryDocument(
                report.Summary.TotalLines,
                report.Summary.ParsedEvents,
                report.Summary.FailedLines,
                report.Summary.FindingsCount,
                report.Summary.SuspiciousEvents,
                report.Summary.OverallRisk.Value,
                report.Summary.OverallRisk.Severity.ToString(),
                report.Summary.CategoryCounts.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value),
                report.Summary.SeverityCounts.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value),
                report.Summary.TopSourceIps.Select(item => new ReportTopItemDocument(item.SourceIp, item.SuspiciousEvents)).ToArray(),
                report.Summary.TopTargetUrls.Select(item => new ReportTopItemDocument(item.Url, item.Count)).ToArray(),
                report.Summary.TopUserAgents.Select(item => new ReportTopItemDocument(item.UserAgent, item.Count)).ToArray(),
                report.Summary.Timeline.Select(item => new ReportTimelineBucketDocument(item.StartsAt, item.EventCount, item.FindingCount)).ToArray()),
            report.Findings.Select(finding => new ReportFindingDocument(
                finding.Id.Value,
                finding.Category.ToString(),
                finding.Severity.ToString(),
                finding.RiskScore.Value,
                finding.RiskScore.Severity.ToString(),
                finding.RiskScore.Components.Select(component => new ReportRiskComponentDocument(component.Name, component.Contribution, component.Explanation)).ToArray(),
                finding.Confidence,
                finding.DetectorId,
                finding.DetectorName,
                finding.DetectionType.ToString(),
                finding.Title,
                finding.Description,
                finding.Reason,
                finding.Evidence.Select(item => new ReportEvidenceDocument(item.Field, item.Value, item.MatchedPattern, item.Explanation)).ToArray(),
                finding.RecommendedAction,
                finding.SourceIp,
                finding.TargetResource,
                finding.Tags)).ToArray(),
            report.Recommendations,
            report.Metadata);
    }
}
