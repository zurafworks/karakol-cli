using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Karakol.Domain.Enums;
using Karakol.Domain.Sources;
using Karakol.Parsing.Input;
using Karakol.Parsing.Options;
using Karakol.Parsing.Parsers.AccessLogs;

namespace Karakol.Benchmarks;

[MemoryDiagnoser]
public class ParserBenchmarks
{
    private readonly NginxAccessLogParser parser = new();
    private ParserOptions options = null!;
    private string[] lines = [];

    [GlobalSetup]
    public void Setup()
    {
        var source = new LogSource(LogSourceId.New(), "benchmark.log", LogFormat.NginxAccess, 0, null, null, LogSourceType.File);
        options = new ParserOptions(source);
        lines = Enumerable.Range(0, 10_000)
            .Select(index => $"127.0.0.1 - - [10/Oct/2026:13:55:36 +0300] \"GET /home/{index} HTTP/1.1\" 200 2326 \"-\" \"Mozilla/5.0\"")
            .ToArray();
    }

    [Benchmark]
    public async Task<int> ParseNginxLines()
    {
        var count = 0;
        await foreach (var result in parser.ParseAsync(ToLines(lines), options, CancellationToken.None))
        {
            if (result.IsSuccess)
            {
                count++;
            }
        }

        return count;
    }

    private static async IAsyncEnumerable<LogLine> ToLines(string[] values, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (var index = 0; index < values.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return new LogLine(index + 1, values[index]);
        }
    }
}
