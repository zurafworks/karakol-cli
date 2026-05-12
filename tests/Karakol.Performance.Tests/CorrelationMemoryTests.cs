using FluentAssertions;
using Karakol.Correlation.Engine;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Sources;

namespace Karakol.Performance.Tests;

public sealed class CorrelationMemoryTests
{
    [Fact]
    [Trait("Category", "MemorySmoke")]
    public void Correlation_related_events_are_bounded()
    {
        var events = Enumerable.Range(0, 2_000)
            .Select(index => new SecurityEvent(SecurityEventId.New(), LogSourceId.New(), LogFormat.NginxAccess, $"GET /login/{index}", index + 1)
            {
                SourceIp = "203.0.113.10",
                Path = "/login",
                StatusCode = 401,
                Timestamp = new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero).AddSeconds(index)
            })
            .ToArray();

        var engine = new CorrelationEngine();
        var findings = engine.Analyze(events, new CorrelationOptions(RelatedEventLimit: 25, BruteForceThreshold: 10, BruteForceCriticalThreshold: 100, DosThreshold: 500));

        findings.Should().NotBeEmpty();
        findings.Should().OnlyContain(finding => finding.RelatedEvents.Count <= 25);
    }
}
