using FluentAssertions;
using Karakol.Domain.Enums;
using Karakol.Domain.Findings;
using Karakol.Domain.Risk;
using Karakol.Domain.Scans;
using Karakol.Risk.Scoring;

namespace Karakol.Risk.Tests;

public sealed class RiskScoringTests
{
    [Fact]
    public void Critical_finding_gets_high_score()
    {
        var score = new DefaultRiskScoringService().CalculateFindingRisk(Finding(Severity.Critical), new RiskContext());

        score.Value.Should().BeGreaterThanOrEqualTo(90);
    }

    [Fact]
    public void Low_confidence_reduces_score()
    {
        var service = new DefaultRiskScoringService();
        var highConfidence = service.CalculateFindingRisk(Finding(Severity.High, confidence: 0.9), new RiskContext());
        var lowConfidence = service.CalculateFindingRisk(Finding(Severity.High, confidence: 0.2), new RiskContext());

        lowConfidence.Value.Should().BeLessThan(highConfidence.Value);
    }

    [Fact]
    public void Sensitive_endpoint_increases_score()
    {
        var service = new DefaultRiskScoringService();
        var regular = service.CalculateFindingRisk(Finding(Severity.High), new RiskContext());
        var sensitive = service.CalculateFindingRisk(Finding(Severity.High), new RiskContext { IsSensitiveEndpoint = true });

        sensitive.Value.Should().BeGreaterThan(regular.Value);
    }

    [Fact]
    public void Trusted_ip_reduces_score()
    {
        var service = new DefaultRiskScoringService();
        var regular = service.CalculateFindingRisk(Finding(Severity.High), new RiskContext());
        var trusted = service.CalculateFindingRisk(Finding(Severity.High), new RiskContext { IsTrustedIp = true });

        trusted.Value.Should().BeLessThan(regular.Value);
        trusted.Components.Should().Contain(component => component.Name == "TrustedIp" && component.Contribution < 0);
    }

    [Fact]
    public void Category_diversity_increases_finding_score()
    {
        var service = new DefaultRiskScoringService();
        var regular = service.CalculateFindingRisk(Finding(Severity.Medium), new RiskContext { CategoryDiversity = 1 });
        var diverse = service.CalculateFindingRisk(Finding(Severity.Medium), new RiskContext { CategoryDiversity = 4 });

        diverse.Value.Should().BeGreaterThan(regular.Value);
        diverse.Components.Should().Contain(component => component.Name == "CategoryDiversity" && component.Contribution > 0);
    }

    [Fact]
    public void Scan_risk_is_capped_at_100()
    {
        var findings = Enumerable.Range(0, 20).Select(_ => Finding(Severity.Critical) with { SourceIp = "1.2.3.4" }).ToArray();
        var summary = Summary(findings);

        var risk = new DefaultRiskScoringService().CalculateScanRisk(summary, findings);

        risk.Value.Should().Be(100);
    }

    [Fact]
    public void Scan_risk_increases_with_category_diversity()
    {
        var findings = new[]
        {
            Finding(Severity.Medium, ThreatCategory.SqlInjection),
            Finding(Severity.Medium, ThreatCategory.Xss),
            Finding(Severity.Medium, ThreatCategory.PathTraversal)
        };
        var summary = Summary(findings);

        var risk = new DefaultRiskScoringService().CalculateScanRisk(summary, findings);

        risk.Components.Should().Contain(component => component.Name == "CategoryDiversity" && component.Contribution > 0);
    }

    private static DetectionFinding Finding(Severity severity, ThreatCategory category = ThreatCategory.SqlInjection, double confidence = 0.9)
    {
        return new DetectionFinding(category, severity, confidence, "rule", "Rule", DetectionType.Rule, "Title", "Description", "Reason", [new Evidence("Url", "/x", "x", "matched")], "Fix");
    }

    private static ScanSummary Summary(IReadOnlyCollection<DetectionFinding> findings)
    {
        return new ScanSummary(
            1,
            1,
            0,
            findings.Count,
            findings.Count,
            new RiskScore(0),
            findings.GroupBy(finding => finding.Category).ToDictionary(group => group.Key, group => group.Count()),
            findings.GroupBy(finding => finding.Severity).ToDictionary(group => group.Key, group => group.Count()),
            [],
            [],
            [],
            [],
            []);
    }
}
