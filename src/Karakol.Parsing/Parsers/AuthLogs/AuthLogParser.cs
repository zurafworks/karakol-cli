using System.Runtime.CompilerServices;
using Karakol.Domain.Enums;
using Karakol.Parsing.Abstractions;
using Karakol.Parsing.Detection;
using Karakol.Parsing.Input;
using Karakol.Parsing.Options;
using Karakol.Parsing.Results;

namespace Karakol.Parsing.Parsers.AuthLogs;

public sealed class AuthLogParser : ILogParser
{
    private readonly SshAuthLogParser _sshParser = new();

    public string FormatName => "auth";

    public LogFormat Format => LogFormat.Auth;

    public int Priority => 75;

    public bool CanParse(LogSample sample) => _sshParser.CanParse(sample);

    public async IAsyncEnumerable<ParseResult> ParseAsync(
        IAsyncEnumerable<LogLine> lines,
        ParserOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var result in _sshParser.ParseAsync(lines, options, cancellationToken).ConfigureAwait(false))
        {
            if (result.Event is null)
            {
                yield return result;
                continue;
            }

            yield return ParseResult.Success(result.Event with { LogFormat = LogFormat.Auth });
        }
    }
}
