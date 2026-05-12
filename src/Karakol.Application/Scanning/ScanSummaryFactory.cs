using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Findings;
using Karakol.Domain.Risk;
using Karakol.Domain.Scans;
using Karakol.Domain.ValueObjects;

namespace Karakol.Application.Scanning;

internal static class ScanSummaryFactory
{
    public static ScanSummary Build(int totalLines, IReadOnlyCollection<SecurityEvent> events, int failedLines, IReadOnlyCollection<DetectionFinding> findings, RiskScore overallRisk)
    {
        var categoryCounts = findings.GroupBy(finding => finding.Category).ToDictionary(group => group.Key, group => group.Count());
        var severityCounts = findings.GroupBy(finding => finding.Severity).ToDictionary(group => group.Key, group => group.Count());
        var topIps = findings.Where(finding => !string.IsNullOrWhiteSpace(finding.SourceIp)).GroupBy(finding => finding.SourceIp!).OrderByDescending(group => group.Count()).Take(10).Select(group => new TopSourceIpSummary(group.Key, group.Count())).ToArray();
        var topUrls = events.Where(securityEvent => !string.IsNullOrWhiteSpace(securityEvent.Path)).GroupBy(securityEvent => securityEvent.Path!).OrderByDescending(group => group.Count()).Take(10).Select(group => new TopTargetUrlSummary(group.Key, group.Count())).ToArray();
        var topUserAgents = events.Where(securityEvent => !string.IsNullOrWhiteSpace(securityEvent.UserAgent)).GroupBy(securityEvent => securityEvent.UserAgent!).OrderByDescending(group => group.Count()).Take(10).Select(group => new TopUserAgentSummary(group.Key, group.Count())).ToArray();
        var timeline = events.GroupBy(securityEvent => securityEvent.Timestamp?.AddSeconds(-securityEvent.Timestamp.Value.Second).AddMilliseconds(-securityEvent.Timestamp.Value.Millisecond)).OrderBy(group => group.Key).Select(group => new TimelineBucket(group.Key, group.Count(), findings.Count(finding => finding.EventId is not null && group.Any(securityEvent => securityEvent.Id == finding.EventId.Value)))).ToArray();
        var recommendations = BuildRecommendations(findings);

        return new ScanSummary(totalLines, events.Count, failedLines, findings.Count, findings.Select(finding => finding.EventId).Distinct().Count(), overallRisk, categoryCounts, severityCounts, topIps, topUrls, topUserAgents, timeline, recommendations);
    }

    private static IReadOnlyCollection<string> BuildRecommendations(IReadOnlyCollection<DetectionFinding> findings)
    {
        var recommendations = new List<string>();
        if (findings.Any(finding => finding.Category == ThreatCategory.SqlInjection))
        {
            recommendations.Add("Review database-backed endpoints and enforce parameterized queries.");
        }

        if (findings.Any(finding => finding.Category == ThreatCategory.Xss))
        {
            recommendations.Add("Validate output encoding and input sanitization on reflected parameters.");
        }

        if (findings.Any(finding => finding.Category is ThreatCategory.ScannerBot or ThreatCategory.SuspiciousUserAgent))
        {
            recommendations.Add("Apply rate limiting and block confirmed scanner user-agents or source IPs.");
        }

        if (findings.Any(finding => finding.Category == ThreatCategory.SensitiveFileAccess))
        {
            recommendations.Add("Ensure sensitive files are outside web roots and blocked at the edge.");
        }

        return recommendations.Count == 0 ? ["No immediate remediation is required based on the analyzed rules."] : recommendations;
    }
}
