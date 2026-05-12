using BenchmarkDotNet.Attributes;
using Karakol.Correlation.Engine;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Sources;

namespace Karakol.Benchmarks;

[MemoryDiagnoser]
public class CorrelationBenchmarks
{
    private readonly CorrelationEngine engine = new();
    private SecurityEvent[] events = [];

    [GlobalSetup]
    public void Setup()
    {
        var start = new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero);
        events = Enumerable.Range(0, 10_000)
            .Select(index => new SecurityEvent(SecurityEventId.New(), LogSourceId.New(), LogFormat.NginxAccess, $"GET /login/{index}", index + 1)
            {
                SourceIp = $"203.0.113.{index % 25}",
                Path = "/login",
                StatusCode = 401,
                Timestamp = start.AddSeconds(index)
            })
            .ToArray();
    }

    [Benchmark]
    public int AnalyzeCorrelation()
    {
        return engine.Analyze(events).Count;
    }
}
