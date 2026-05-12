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

namespace Karakol.Parsing.Parsers.AccessLogs;

public abstract class AccessLogParserBase : ILogParser
{
    private static readonly Regex AccessLogRegex = new(
        "^(?<ip>\\S+) \\S+ \\S+ \\[(?<timestamp>[^\\]]+)\\] \"(?<request>(?:\\\\.|[^\"])*)\" (?<status>\\d{3}) (?<size>\\S+)(?: \"(?<referrer>(?:\\\\.|[^\"])*)\" \"(?<ua>(?:\\\\.|[^\"])*)\")?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public abstract string FormatName { get; }

    public abstract LogFormat Format { get; }

    public abstract int Priority { get; }

    public bool CanParse(LogSample sample) => sample.Lines.Any(line => AccessLogRegex.IsMatch(line));

    public async IAsyncEnumerable<ParseResult> ParseAsync(
        IAsyncEnumerable<LogLine> lines,
        ParserOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var line in lines.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var match = AccessLogRegex.Match(line.Content);
            if (!match.Success)
            {
                yield return ParseResult.Failure(line.LineNumber, line.Content, "Line does not match access log format.");
                continue;
            }

            var timestamp = ParseAccessTimestamp(match.Groups["timestamp"].Value);
            if (timestamp is null)
            {
                yield return ParseResult.Failure(line.LineNumber, line.Content, "Access log timestamp is invalid.");
                continue;
            }

            if (!TryParseRequest(match.Groups["request"].Value, out var method, out var url, out var protocol))
            {
                yield return ParseResult.Failure(line.LineNumber, line.Content, "Access log request is invalid.");
                continue;
            }

            var questionIndex = url.IndexOf('?', StringComparison.Ordinal);
            var path = questionIndex >= 0 ? url[..questionIndex] : url;
            var query = questionIndex >= 0 && questionIndex + 1 < url.Length ? url[(questionIndex + 1)..] : null;
            _ = int.TryParse(match.Groups["status"].Value, CultureInfo.InvariantCulture, out var statusCode);
            var sizeValue = match.Groups["size"].Value;
            long? size = long.TryParse(sizeValue, CultureInfo.InvariantCulture, out var parsedSize) ? parsedSize : null;

            yield return ParseResult.Success(new SecurityEvent(SecurityEventId.New(), options.Source.Id, Format, line.Content, line.LineNumber)
            {
                SourceIp = match.Groups["ip"].Value,
                Timestamp = timestamp,
                HttpMethod = method,
                Url = url,
                Path = path,
                QueryString = query,
                Protocol = EmptyToNull(protocol),
                StatusCode = statusCode,
                ResponseSize = size,
                Referrer = EmptyToNull(UnescapeQuotedValue(match.Groups["referrer"].Value)),
                UserAgent = EmptyToNull(UnescapeQuotedValue(match.Groups["ua"].Value)),
                NormalizedMessage = Normalize(line.Content)
            });
        }
    }

    protected static string Normalize(string value) => Uri.UnescapeDataString(value).ToLowerInvariant();

    private static DateTimeOffset? ParseAccessTimestamp(string value)
    {
        return DateTimeOffset.TryParseExact(value, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) || value == "-" ? null : value;

    private static bool TryParseRequest(string value, out string method, out string url, out string? protocol)
    {
        method = string.Empty;
        url = string.Empty;
        protocol = null;

        var firstSpace = value.IndexOf(' ', StringComparison.Ordinal);
        if (firstSpace <= 0 || firstSpace == value.Length - 1)
        {
            return false;
        }

        method = value[..firstSpace];
        var remainder = value[(firstSpace + 1)..];
        var lastSpace = remainder.LastIndexOf(" ", StringComparison.Ordinal);
        if (lastSpace <= 0)
        {
            url = UnescapeQuotedValue(remainder);
            return !string.IsNullOrWhiteSpace(url);
        }

        url = UnescapeQuotedValue(remainder[..lastSpace]);
        protocol = remainder[(lastSpace + 1)..];
        return !string.IsNullOrWhiteSpace(url);
    }

    private static string UnescapeQuotedValue(string value)
    {
        return value.Replace("\\\"", "\"", StringComparison.Ordinal).Replace("\\\\", "\\", StringComparison.Ordinal);
    }
}
