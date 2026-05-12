using System.Diagnostics;
using FluentAssertions;
using Karakol.Domain.Enums;
using Karakol.Domain.Sources;
using Karakol.Parsing.Input;

namespace Karakol.Performance.Tests;

public sealed class StreamingSmokeTests
{
    [Fact]
    [Trait("Category", "PerformanceSmoke")]
    public async Task Physical_reader_streams_100k_lines_with_reasonable_time()
    {
        var path = await CreateLogAsync(100_000);
        var source = new LogSource(LogSourceId.New(), path, LogFormat.NginxAccess, new FileInfo(path).Length, null, null, LogSourceType.File);
        var reader = new PhysicalLogInputReader();
        var stopwatch = Stopwatch.StartNew();
        var count = 0;

        await foreach (var _ in reader.ReadLinesAsync(source, new ReadOptions(), CancellationToken.None))
        {
            count++;
        }

        stopwatch.Stop();
        count.Should().Be(100_000);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
    }

    [Fact]
    [Trait("Category", "PerformanceSmoke")]
    public async Task Physical_reader_honors_max_lines_without_reading_rest()
    {
        var path = await CreateLogAsync(100_000);
        var source = new LogSource(LogSourceId.New(), path, LogFormat.NginxAccess, new FileInfo(path).Length, null, null, LogSourceType.File);
        var reader = new PhysicalLogInputReader();
        var count = 0;

        await foreach (var _ in reader.ReadLinesAsync(source, new ReadOptions(MaxLines: 123), CancellationToken.None))
        {
            count++;
        }

        count.Should().Be(123);
    }

    private static async Task<string> CreateLogAsync(int lineCount)
    {
        return await LargeLogFixture.CreateAsync(lineCount);
    }
}
