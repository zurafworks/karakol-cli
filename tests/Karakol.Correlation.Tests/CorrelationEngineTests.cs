using FluentAssertions;
using Karakol.Correlation.Engine;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Sources;

namespace Karakol.Correlation.Tests;

public sealed class CorrelationEngineTests
{
    [Fact]
    public void Brute_force_uses_time_window_threshold()
    {
        var start = DateTimeOffset.Parse("2026-01-01T10:00:00Z");
        var events = Enumerable.Range(0, 10)
            .Select(index => FailedLogin("1.2.3.4", start.AddSeconds(index * 10)))
            .Concat([FailedLogin("1.2.3.4", start.AddMinutes(20))])
            .ToArray();

        var findings = new CorrelationEngine().Analyze(events, new CorrelationOptions(BruteForceWindow: TimeSpan.FromMinutes(5), BruteForceThreshold: 10));

        findings.Should().ContainSingle(finding => finding.Category == ThreatCategory.BruteForce);
    }

    [Fact]
    public void Brute_force_ignores_events_outside_time_window()
    {
        var start = DateTimeOffset.Parse("2026-01-01T10:00:00Z");
        var events = Enumerable.Range(0, 10)
            .Select(index => FailedLogin("1.2.3.4", start.AddMinutes(index)))
            .ToArray();

        var findings = new CorrelationEngine().Analyze(events, new CorrelationOptions(BruteForceWindow: TimeSpan.FromMinutes(2), BruteForceThreshold: 10));

        findings.Should().NotContain(finding => finding.Category == ThreatCategory.BruteForce);
    }

    [Fact]
    public void Dos_detection_limits_related_events()
    {
        var start = DateTimeOffset.Parse("2026-01-01T10:00:00Z");
        var events = Enumerable.Range(0, 20)
            .Select(index => Request("5.6.7.8", start.AddSeconds(index)))
            .ToArray();

        var findings = new CorrelationEngine().Analyze(events, new CorrelationOptions(DosWindow: TimeSpan.FromSeconds(30), DosThreshold: 10, RelatedEventLimit: 7));

        findings.Should().ContainSingle(finding => finding.Category == ThreatCategory.DosAttempt);
        findings.Single(finding => finding.Category == ThreatCategory.DosAttempt).RelatedEvents.Should().HaveCount(7);
    }

    private static SecurityEvent FailedLogin(string sourceIp, DateTimeOffset timestamp)
    {
        return new SecurityEvent(SecurityEventId.New(), LogSourceId.New(), LogFormat.Auth, $"failed {sourceIp}", 1)
        {
            SourceIp = sourceIp,
            Timestamp = timestamp,
            EventType = "failed_login"
        };
    }

    private static SecurityEvent Request(string sourceIp, DateTimeOffset timestamp)
    {
        return new SecurityEvent(SecurityEventId.New(), LogSourceId.New(), LogFormat.NginxAccess, $"GET / {sourceIp}", 1)
        {
            SourceIp = sourceIp,
            Timestamp = timestamp,
            Path = "/home",
            StatusCode = 200
        };
    }
}
