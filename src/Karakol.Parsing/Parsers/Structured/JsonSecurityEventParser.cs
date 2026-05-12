using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Parsing.Abstractions;
using Karakol.Parsing.Detection;
using Karakol.Parsing.Input;
using Karakol.Parsing.Options;
using Karakol.Parsing.Results;

namespace Karakol.Parsing.Parsers.Structured;

public sealed class JsonSecurityEventParser : ILogParser
{
    private static readonly HashSet<string> KnownProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "timestamp",
        "sourceIp",
        "source_ip",
        "ip",
        "client_ip",
        "method",
        "httpMethod",
        "url",
        "path",
        "status",
        "statusCode",
        "status_code",
        "userAgent",
        "user_agent",
        "username",
        "user"
    };

    public string FormatName => "json";

    public LogFormat Format => LogFormat.Json;

    public int Priority => 70;

    public bool CanParse(LogSample sample) => sample.Lines.Any(line =>
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith('{') || trimmed.StartsWith('[');
    });

    public async IAsyncEnumerable<ParseResult> ParseAsync(
        IAsyncEnumerable<LogLine> lines,
        ParserOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        StringBuilder? pendingJsonArray = null;
        int pendingLineNumber = 0;
        string? pendingParseError = null;

        await foreach (var line in lines.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (pendingJsonArray is not null)
            {
                pendingJsonArray.AppendLine(line.Content);
                if (!TryParse(pendingJsonArray.ToString(), out var pendingDocument, out pendingParseError))
                {
                    continue;
                }

                using (pendingDocument)
                {
                    foreach (var result in ProcessDocument(pendingDocument!, options, pendingJsonArray.ToString(), pendingLineNumber))
                    {
                        yield return result;
                    }
                }

                pendingJsonArray = null;
                pendingLineNumber = 0;
                pendingParseError = null;
                continue;
            }

            if (!TryParse(line.Content, out var document, out var parseError))
            {
                if (line.Content.TrimStart().StartsWith("[", StringComparison.Ordinal))
                {
                    pendingJsonArray = new StringBuilder();
                    pendingJsonArray.AppendLine(line.Content);
                    pendingLineNumber = line.LineNumber;
                    pendingParseError = parseError;
                    continue;
                }

                yield return ParseResult.Failure(line.LineNumber, line.Content, parseError);
                continue;
            }

            using (document)
            {
                foreach (var result in ProcessDocument(document!, options, line.Content, line.LineNumber))
                {
                    yield return result;
                }
            }
        }

        if (pendingJsonArray is not null)
        {
            yield return ParseResult.Failure(pendingLineNumber, pendingJsonArray.ToString(), pendingParseError ?? "Invalid JSON array.");
        }
    }

    private SecurityEvent CreateEvent(JsonElement root, ParserOptions options, string rawMessage, int lineNumber)
    {
        var url = GetString(root, "url") ?? GetString(root, "path");
        var path = url?.Split('?', 2)[0];
        var query = url is not null && url.Contains('?', StringComparison.Ordinal) ? url.Split('?', 2)[1] : null;

        return new SecurityEvent(SecurityEventId.New(), options.Source.Id, Format, rawMessage, lineNumber)
        {
            Timestamp = TryTimestamp(GetString(root, "timestamp")),
            SourceIp = GetString(root, "sourceIp") ?? GetString(root, "source_ip") ?? GetString(root, "ip") ?? GetString(root, "client_ip"),
            HttpMethod = GetString(root, "method") ?? GetString(root, "httpMethod"),
            Url = url,
            Path = path,
            QueryString = query,
            StatusCode = GetInt(root, "status") ?? GetInt(root, "statusCode") ?? GetInt(root, "status_code"),
            UserAgent = GetString(root, "userAgent") ?? GetString(root, "user_agent"),
            Username = GetString(root, "username") ?? GetString(root, "user"),
            NormalizedMessage = Uri.UnescapeDataString(rawMessage).ToLowerInvariant(),
            Metadata = root.EnumerateObject()
                .Where(property => !KnownProperties.Contains(property.Name))
                .ToDictionary(property => property.Name, property => property.Value.ToString(), StringComparer.OrdinalIgnoreCase)
        };
    }

    private IEnumerable<ParseResult> ProcessDocument(JsonDocument document, ParserOptions options, string rawMessage, int lineNumber)
    {
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
        {
            var itemIndex = 0;
            foreach (var item in root.EnumerateArray())
            {
                itemIndex++;
                if (item.ValueKind != JsonValueKind.Object)
                {
                    yield return ParseResult.Failure(lineNumber, item.ToString(), $"JSON array item {itemIndex} is not an object.");
                    continue;
                }

                yield return ParseResult.Success(CreateEvent(item, options, item.GetRawText(), lineNumber));
            }

            yield break;
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            yield return ParseResult.Failure(lineNumber, rawMessage, "JSON root must be an object or an array of objects.");
            yield break;
        }

        yield return ParseResult.Success(CreateEvent(root, options, rawMessage, lineNumber));
    }

    private static bool TryParse(string content, out JsonDocument? document, out string parseError)
    {
        try
        {
            document = JsonDocument.Parse(content);
            parseError = string.Empty;
            return true;
        }
        catch (JsonException ex)
        {
            document = null;
            parseError = ex.Message;
            return false;
        }
    }

    private static string? GetString(JsonElement root, string name) => root.TryGetProperty(name, out var property) ? property.ToString() : null;

    private static int? GetInt(JsonElement root, string name) => root.TryGetProperty(name, out var property) && property.TryGetInt32(out var value) ? value : null;

    private static DateTimeOffset? TryTimestamp(string? value) => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed) ? parsed : null;
}
