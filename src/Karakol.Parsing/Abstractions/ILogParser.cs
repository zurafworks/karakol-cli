using Karakol.Domain.Enums;
using Karakol.Parsing.Detection;
using Karakol.Parsing.Input;
using Karakol.Parsing.Options;
using Karakol.Parsing.Results;

namespace Karakol.Parsing.Abstractions;

public interface ILogParser
{
    string FormatName { get; }

    LogFormat Format { get; }

    int Priority { get; }

    bool CanParse(LogSample sample);

    IAsyncEnumerable<ParseResult> ParseAsync(IAsyncEnumerable<LogLine> lines, ParserOptions options, CancellationToken cancellationToken);
}
