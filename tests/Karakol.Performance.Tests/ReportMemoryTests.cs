using FluentAssertions;
using Karakol.Domain.Enums;
using Karakol.Domain.Risk;
using Karakol.Domain.Scans;
using Karakol.Domain.Sources;
using Karakol.Domain.ValueObjects;
using Karakol.Reporting.Writers.Json;

namespace Karakol.Performance.Tests;

public sealed class ReportMemoryTests
{
    [Fact]
    [Trait("Category", "MemorySmoke")]
    public async Task Json_report_generation_memory_growth_stays_bounded_for_empty_large_summary()
    {
        var report = CreateReport(totalLines: 500_000);
        var output = Path.Combine(Path.GetTempPath(), "karakol-performance-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);
        var writer = new JsonReportWriter();

        var memoryGrowth = await MemoryMeasurement.MeasureAllocatedBytesAsync(async () =>
        {
            await writer.WriteAsync(report, new Karakol.Reporting.Options.ReportOptions(output, MaskSensitiveData: true, IncludeRawSamples: false), CancellationToken.None);
        });

        memoryGrowth.Should().BeLessThan(16 * 1024 * 1024);
    }

    private static ScanReport CreateReport(int totalLines)
    {
        var summary = new ScanSummary(
            totalLines,
            totalLines,
            0,
            0,
            0,
            new RiskScore(5),
            new Dictionary<ThreatCategory, int>(),
            new Dictionary<Severity, int>(),
            [],
            [],
            [],
            [new TimelineBucket(new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero), totalLines, 0)],
            []);

        return new ScanReport(
            ScanId.New(),
            "Karakol",
            "0.1.0",
            DateTimeOffset.UtcNow,
            new LogSource(LogSourceId.New(), "large.log", LogFormat.NginxAccess, 0, null, null, LogSourceType.File),
            summary,
            [],
            summary.Timeline,
            summary.Recommendations,
            new ScanConfigurationSnapshot(LogFormat.NginxAccess, ["json"], true, false, true, true, null),
            new Dictionary<string, string>());
    }
}
