using BenchmarkDotNet.Attributes;
using Karakol.Detection.Context;
using Karakol.Detection.Options;
using Karakol.Detection.Rules.Injection;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Sources;

namespace Karakol.Benchmarks;

[MemoryDiagnoser]
public class DetectionRuleBenchmarks
{
    private readonly SqlInjectionRule rule = new();
    private readonly RuleExecutionContext context = new(new RuleOptions());
    private SecurityEvent[] events = [];

    [GlobalSetup]
    public void Setup()
    {
        events = Enumerable.Range(0, 10_000)
            .Select(index => Event(index % 10 == 0 ? "/login?id=1 union select password" : $"/products?id={index}"))
            .ToArray();
    }

    [Benchmark]
    public int AnalyzeSqlInjectionRule()
    {
        var findings = 0;
        foreach (var securityEvent in events)
        {
            if (rule.Analyze(securityEvent, context) is not null)
            {
                findings++;
            }
        }

        return findings;
    }

    private static SecurityEvent Event(string url)
    {
        return new SecurityEvent(SecurityEventId.New(), LogSourceId.New(), LogFormat.NginxAccess, $"GET {url}", 1)
        {
            Url = url,
            Path = url.Split('?', 2)[0],
            QueryString = url.Contains('?', StringComparison.Ordinal) ? url.Split('?', 2)[1] : null,
            UserAgent = "Mozilla/5.0",
            NormalizedMessage = Uri.UnescapeDataString(url).ToLowerInvariant()
        };
    }
}
