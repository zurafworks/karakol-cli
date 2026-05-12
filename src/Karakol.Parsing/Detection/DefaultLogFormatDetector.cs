using Karakol.Domain.Enums;
using Karakol.Domain.Sources;
using Karakol.Parsing.Abstractions;
using Karakol.Parsing.Input;

namespace Karakol.Parsing.Detection;

public sealed class DefaultLogFormatDetector(IParserRegistry parserRegistry, ILogInputReader inputReader) : ILogFormatDetector
{
    public async Task<FormatDetectionResult> DetectAsync(LogSource source, CancellationToken cancellationToken)
    {
        var lines = new List<string>();
        await foreach (var line in inputReader.ReadLinesAsync(source, new ReadOptions(MaxLines: 25), cancellationToken).ConfigureAwait(false))
        {
            lines.Add(line.Content);
        }

        var sample = new LogSample(lines);
        var evidence = new List<string>();
        foreach (var parser in parserRegistry.GetParsers().Where(parser => parser.Format != LogFormat.Generic))
        {
            if (parser.CanParse(sample))
            {
                var confidence = CalculateConfidence(parser, sample);
                return new FormatDetectionResult(parser.Format, confidence, [$"{parser.FormatName} matched the sample with confidence {confidence:0.00}."]);
            }

            evidence.Add($"{parser.FormatName} did not match.");
        }

        return new FormatDetectionResult(LogFormat.Generic, 0.25, ["No strong parser match; using generic parser.", .. evidence.Take(5)]);
    }

    private static double CalculateConfidence(ILogParser parser, LogSample sample)
    {
        if (sample.Lines.Count == 0)
        {
            return 0;
        }

        return parser.Format switch
        {
            LogFormat.Json when sample.Lines.Count(line => line.TrimStart().StartsWith('{') || line.TrimStart().StartsWith('[')) == sample.Lines.Count => 0.95,
            LogFormat.Csv when sample.Lines.FirstOrDefault()?.Contains(',', StringComparison.Ordinal) == true => 0.85,
            _ => 0.90
        };
    }
}
