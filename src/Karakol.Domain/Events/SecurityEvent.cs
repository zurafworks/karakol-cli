using Karakol.Domain.Enums;
using Karakol.Domain.Sources;
using Karakol.Shared.Guards;

namespace Karakol.Domain.Events;

public sealed record SecurityEvent
{
    public SecurityEvent(SecurityEventId id, LogSourceId sourceId, LogFormat logFormat, string rawMessage, int lineNumber)
    {
        Id = id;
        SourceId = sourceId;
        LogFormat = logFormat;
        RawMessage = Guard.NotNullOrWhiteSpace(rawMessage, nameof(rawMessage));
        LineNumber = Guard.AtLeast(lineNumber, 1, nameof(lineNumber));
    }

    public SecurityEventId Id { get; init; }
    public LogSourceId SourceId { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
    public string? SourceIp { get; init; }
    public string? DestinationIp { get; init; }
    public int? SourcePort { get; init; }
    public int? DestinationPort { get; init; }
    public string? Protocol { get; init; }
    public string? HttpMethod { get; init; }
    public string? Url { get; init; }
    public string? Path { get; init; }
    public string? QueryString { get; init; }
    public int? StatusCode { get; init; }
    public long? ResponseSize { get; init; }
    public string? UserAgent { get; init; }
    public string? Referrer { get; init; }
    public string? Username { get; init; }
    public string? Hostname { get; init; }
    public string? ProcessName { get; init; }
    public string? EventType { get; init; }
    public LogFormat LogFormat { get; init; }
    public string RawMessage { get; init; }
    public string? NormalizedMessage { get; init; }
    public int LineNumber { get; init; }
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
