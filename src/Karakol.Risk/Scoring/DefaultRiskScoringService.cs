using Karakol.Domain.Enums;
using Karakol.Domain.Findings;
using Karakol.Domain.Risk;
using Karakol.Domain.Scans;
using Karakol.Risk.Abstractions;

namespace Karakol.Risk.Scoring;

public sealed class DefaultRiskScoringService : IRiskScoringService
{
    public RiskScore CalculateFindingRisk(DetectionFinding finding, RiskContext context)
    {
        var baseScore = finding.Severity switch
        {
            Severity.Info => 5,
            Severity.Low => 20,
            Severity.Medium => 50,
            Severity.High => 75,
            Severity.Critical => 95,
            _ => 10
        };

        var components = new List<RiskComponent> { new("Severity", baseScore, $"{finding.Severity} finding base score.") };

        if (finding.Confidence < 0.7)
        {
            components.Add(new RiskComponent("Confidence", -10, "Lower confidence reduces the score."));
            baseScore -= 10;
        }

        if (finding.DetectionType == DetectionType.Correlation)
        {
            components.Add(new RiskComponent("Correlation", 8, "Correlated activity increases risk."));
            baseScore += 8;
        }

        if (context.IsSensitiveEndpoint)
        {
            components.Add(new RiskComponent("SensitiveEndpoint", 8, "Sensitive endpoint or file was touched."));
            baseScore += 8;
        }

        if (context.IsTrustedIp)
        {
            components.Add(new RiskComponent("TrustedIp", -15, "Trusted source IP reduces the score."));
            baseScore -= 15;
        }

        if (context.RepetitionCount > 1)
        {
            var boost = Math.Min(12, context.RepetitionCount * 2);
            components.Add(new RiskComponent("Repetition", boost, "Repeated activity from the same source increases risk."));
            baseScore += boost;
        }

        if (context.CategoryDiversity > 1)
        {
            var boost = Math.Min(10, context.CategoryDiversity * 2);
            components.Add(new RiskComponent("CategoryDiversity", boost, "Multiple threat categories from the same source increase risk."));
            baseScore += boost;
        }

        return new RiskScore(baseScore, components, $"{finding.Category} assessed as {RiskScore.FromValue(baseScore)}.");
    }

    public RiskScore CalculateScanRisk(ScanSummary summary, IReadOnlyCollection<DetectionFinding> findings)
    {
        var critical = summary.SeverityCounts.GetValueOrDefault(Severity.Critical);
        var high = summary.SeverityCounts.GetValueOrDefault(Severity.High);
        var medium = summary.SeverityCounts.GetValueOrDefault(Severity.Medium);
        var low = summary.SeverityCounts.GetValueOrDefault(Severity.Low);
        var repeatedSources = findings.Where(finding => !string.IsNullOrWhiteSpace(finding.SourceIp)).GroupBy(finding => finding.SourceIp).Count(group => group.Count() > 1);
        var sensitive = findings.Count(finding => finding.Category == ThreatCategory.SensitiveFileAccess);
        var categoryDiversity = findings.Select(finding => finding.Category).Distinct().Count();
        var score = critical * 25 + high * 10 + medium * 4 + low + repeatedSources * 5 + categoryDiversity * 3 + sensitive * 5;

        var components = new[]
        {
            new RiskComponent("SeverityCounts", critical * 25 + high * 10 + medium * 4 + low, "Weighted finding severities."),
            new RiskComponent("RepeatedSources", repeatedSources * 5, "Repeated source IP activity."),
            new RiskComponent("CategoryDiversity", categoryDiversity * 3, "Multiple threat categories detected."),
            new RiskComponent("SensitiveFiles", sensitive * 5, "Sensitive file access attempts.")
        };

        return new RiskScore(score, components, "Overall scan risk derived from finding severity, repetition, and category diversity.");
    }
}
