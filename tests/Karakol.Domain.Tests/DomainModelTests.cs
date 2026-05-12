using FluentAssertions;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Findings;
using Karakol.Domain.Risk;
using Karakol.Domain.Sources;

namespace Karakol.Domain.Tests;

public sealed class DomainModelTests
{
    [Theory]
    [InlineData(-10, 0, Severity.Info)]
    [InlineData(9, 9, Severity.Info)]
    [InlineData(10, 10, Severity.Low)]
    [InlineData(40, 40, Severity.Medium)]
    [InlineData(70, 70, Severity.High)]
    [InlineData(90, 90, Severity.Critical)]
    [InlineData(150, 100, Severity.Critical)]
    public void RiskScore_clamps_value_and_maps_severity(int input, int expectedValue, Severity expectedSeverity)
    {
        var score = new RiskScore(input);

        score.Value.Should().Be(expectedValue);
        score.Severity.Should().Be(expectedSeverity);
    }

    [Fact]
    public void SecurityEvent_requires_raw_message()
    {
        var action = () => new SecurityEvent(SecurityEventId.New(), LogSourceId.New(), LogFormat.Generic, "", 1);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SecurityEvent_requires_positive_line_number()
    {
        var action = () => new SecurityEvent(SecurityEventId.New(), LogSourceId.New(), LogFormat.Generic, "raw", 0);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SecurityEvent_initializes_collections()
    {
        var securityEvent = new SecurityEvent(SecurityEventId.New(), LogSourceId.New(), LogFormat.Generic, "raw", 1);

        securityEvent.Tags.Should().NotBeNull();
        securityEvent.Metadata.Should().NotBeNull();
    }

    [Fact]
    public void DetectionFinding_clamps_confidence()
    {
        var finding = new DetectionFinding(
            ThreatCategory.SqlInjection,
            Severity.High,
            5,
            "rule",
            "Rule",
            DetectionType.Rule,
            "Title",
            "Description",
            "Reason",
            [new Evidence("Url", "/login", "union select", "matched")],
            "Fix it");

        finding.Confidence.Should().Be(1);
        finding.RiskScore.Value.Should().Be(0);
    }

    [Fact]
    public void DetectionFinding_requires_detector_id()
    {
        var action = () => new DetectionFinding(
            ThreatCategory.SqlInjection,
            Severity.High,
            0.8,
            "",
            "Rule",
            DetectionType.Rule,
            "Title",
            "Description",
            "Reason",
            [],
            "Fix it");

        action.Should().Throw<ArgumentException>();
    }
}
