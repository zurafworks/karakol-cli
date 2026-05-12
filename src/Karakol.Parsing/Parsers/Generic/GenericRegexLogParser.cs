using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Parsing.Abstractions;
using Karakol.Parsing.Detection;
using Karakol.Parsing.Input;
using Karakol.Parsing.Options;
using Karakol.Parsing.Results;

namespace Karakol.Parsing.Parsers.Generic;

public sealed class GenericRegexLogParser : ILogParser
{
    private static readonly Regex IpRegex = new("\\b(?:\\d{1,3}\\.){3}\\d{1,3}\\b", RegexOptions.Compiled);

    public string FormatName => "generic";

    public LogFormat Format => LogFormat.Generic;

    public int Priority => 1;

    public bool CanParse(LogSample sample) => sample.Lines.Count > 0;

    public async IAsyncEnumerable<ParseResult> ParseAsync(
        IAsyncEnumerable<LogLine> lines,
        ParserOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var line in lines.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var ip = IpRegex.Match(line.Content);
            yield return ParseResult.Success(new SecurityEvent(SecurityEventId.New(), options.Source.Id, Format, line.Content, line.LineNumber)
            {
                SourceIp = ip.Success ? ip.Value : null,
                NormalizedMessage = Uri.UnescapeDataString(line.Content).ToLowerInvariant()
            });
        }
    }
}
