using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Parsing.Abstractions;
using Karakol.Parsing.Detection;
using Karakol.Parsing.Input;
using Karakol.Parsing.Options;
using Karakol.Parsing.Results;

namespace Karakol.Parsing.Parsers.AuthLogs;

public sealed class SshAuthLogParser : ILogParser
{
    private static readonly Regex SshRegex = new(
        "^(?<month>\\w{3})\\s+(?<day>\\d{1,2})\\s+(?<time>\\d{2}:\\d{2}:\\d{2})\\s+(?<host>\\S+)\\s+(?<process>sshd)\\[\\d+\\]:\\s+(?<event>Failed password|Accepted password).*?for(?: invalid user)? (?<user>\\S+) from (?<ip>\\S+) port (?<port>\\d+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public string FormatName => "ssh-auth";

    public LogFormat Format => LogFormat.SshAuth;

    public int Priority => 80;

    public bool CanParse(LogSample sample) => sample.Lines.Any(line => SshRegex.IsMatch(line));

    public async IAsyncEnumerable<ParseResult> ParseAsync(
        IAsyncEnumerable<LogLine> lines,
        ParserOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var line in lines.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var match = SshRegex.Match(line.Content);
            if (!match.Success)
            {
                yield return ParseResult.Failure(line.LineNumber, line.Content, "Line does not match SSH auth format.");
                continue;
            }

            _ = int.TryParse(match.Groups["port"].Value, CultureInfo.InvariantCulture, out var port);
            var failed = match.Groups["event"].Value.StartsWith("Failed", StringComparison.OrdinalIgnoreCase);

            yield return ParseResult.Success(new SecurityEvent(SecurityEventId.New(), options.Source.Id, Format, line.Content, line.LineNumber)
            {
                Hostname = match.Groups["host"].Value,
                ProcessName = match.Groups["process"].Value,
                EventType = failed ? "failed_login" : "accepted_login",
                Username = match.Groups["user"].Value,
                SourceIp = match.Groups["ip"].Value,
                SourcePort = port,
                NormalizedMessage = Uri.UnescapeDataString(line.Content).ToLowerInvariant()
            });
        }
    }
}
