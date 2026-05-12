using System.Diagnostics;
using FluentAssertions;
using Karakol.Domain.Enums;
using Karakol.Domain.Sources;
using Karakol.Parsing.Input;

namespace Karakol.Performance.Tests;

public sealed class LargeLogPerformanceTests
{
    [Fact]
    [Trait("Category", "ManualPerformance")]
    public async Task Physical_reader_honors_max_lines_on_1m_fixture_when_enabled()
    {
        if (!ManualPerformanceEnabled())
        {
            return;
        }

        var path = await LargeLogFixture.CreateAsync(1_000_000);
        var source = new LogSource(LogSourceId.New(), path, LogFormat.NginxAccess, new FileInfo(path).Length, null, null, LogSourceType.File);
        var reader = new PhysicalLogInputReader();
        var stopwatch = Stopwatch.StartNew();
        var count = 0;

        await foreach (var _ in reader.ReadLinesAsync(source, new ReadOptions(MaxLines: 10_000), CancellationToken.None))
        {
            count++;
        }

        stopwatch.Stop();
        count.Should().Be(10_000);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "MemorySmoke")]
    public async Task Physical_reader_streaming_memory_growth_stays_bounded()
    {
        var path = await LargeLogFixture.CreateAsync(50_000);
        var source = new LogSource(LogSourceId.New(), path, LogFormat.NginxAccess, new FileInfo(path).Length, null, null, LogSourceType.File);
        var reader = new PhysicalLogInputReader();

        var memoryGrowth = await MemoryMeasurement.MeasureAllocatedBytesAsync(async () =>
        {
            var count = 0;
            await foreach (var _ in reader.ReadLinesAsync(source, new ReadOptions(), CancellationToken.None))
            {
                count++;
            }

            count.Should().Be(50_000);
        });

        memoryGrowth.Should().BeLessThan(8 * 1024 * 1024);
    }

    private static bool ManualPerformanceEnabled()
    {
        return string.Equals(Environment.GetEnvironmentVariable("KARAKOL_RUN_LARGE_PERF"), "1", StringComparison.Ordinal);
    }
}
