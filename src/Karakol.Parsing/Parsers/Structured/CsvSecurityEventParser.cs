using System.Globalization;
using System.Runtime.CompilerServices;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Parsing.Abstractions;
using Karakol.Parsing.Detection;
using Karakol.Parsing.Input;
using Karakol.Parsing.Options;
using Karakol.Parsing.Results;

namespace Karakol.Parsing.Parsers.Structured;

public sealed class CsvSecurityEventParser : ILogParser
{
    private static readonly HashSet<string> KnownHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "timestamp",
        "sourceip",
        "source_ip",
        "ip",
        "client_ip",
        "useragent",
        "user_agent",
        "status",
        "statuscode",
        "status_code",
        "method",
        "url",
        "path",
        "username",
        "user"
    };

    public string FormatName => "csv";

    public LogFormat Format => LogFormat.Csv;

    public int Priority => 60;

    public bool CanParse(LogSample sample) => sample.Lines.FirstOrDefault()?.Contains(',', StringComparison.Ordinal) == true;

    public async IAsyncEnumerable<ParseResult> ParseAsync(
        IAsyncEnumerable<LogLine> lines,
        ParserOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string[]? headers = null;

        await foreach (var line in lines.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var parts = SplitCsv(line.Content);
            if (headers is null)
            {
                headers = parts.Select(part => part.Trim()).ToArray();
                continue;
            }

            var values = headers
                .Zip(parts, (header, value) => (header: NormalizeHeader(header, options.ColumnMappings), value: value.Trim()))
                .ToDictionary(pair => pair.header, pair => pair.value);
            values.TryGetValue("url", out var url);
            values.TryGetValue("path", out var path);
            values.TryGetValue("sourceip", out var sourceIp);
            values.TryGetValue("source_ip", out var sourceIpSnake);
            values.TryGetValue("ip", out var ip);
            values.TryGetValue("useragent", out var userAgent);
            values.TryGetValue("user_agent", out var userAgentSnake);
            values.TryGetValue("status", out var status);
            values.TryGetValue("statuscode", out var statusCodeValue);
            values.TryGetValue("status_code", out var statusCodeSnake);
            values.TryGetValue("method", out var method);
            _ = int.TryParse(status, CultureInfo.InvariantCulture, out var statusCode);
            if (statusCode == 0)
            {
                _ = int.TryParse(statusCodeValue ?? statusCodeSnake, CultureInfo.InvariantCulture, out statusCode);
            }

            yield return ParseResult.Success(new SecurityEvent(SecurityEventId.New(), options.Source.Id, Format, line.Content, line.LineNumber)
            {
                SourceIp = sourceIp ?? sourceIpSnake ?? ip,
                HttpMethod = method,
                Url = url ?? path,
                Path = path ?? url?.Split('?', 2)[0],
                QueryString = url is not null && url.Contains('?', StringComparison.Ordinal) ? url.Split('?', 2)[1] : null,
                StatusCode = statusCode == 0 ? null : statusCode,
                UserAgent = userAgent ?? userAgentSnake,
                NormalizedMessage = Uri.UnescapeDataString(line.Content).ToLowerInvariant(),
                Metadata = values
                    .Where(pair => !KnownHeaders.Contains(pair.Key))
                    .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase)
            });
        }
    }

    private static string[] SplitCsv(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    current.Append('"');
                    index++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (character == ',' && !inQuotes)
            {
                values.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        values.Add(current.ToString().Trim());
        return values.ToArray();
    }

    private static string NormalizeHeader(string header, IReadOnlyDictionary<string, string>? mappings)
    {
        var normalized = header.Trim().ToLowerInvariant();
        if (mappings is null)
        {
            return normalized;
        }

        foreach (var mapping in mappings)
        {
            if (string.Equals(mapping.Key, header, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mapping.Key, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return mapping.Value.Trim().ToLowerInvariant();
            }
        }

        return normalized;
    }
}
