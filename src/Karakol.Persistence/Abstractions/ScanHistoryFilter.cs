namespace Karakol.Persistence.Abstractions;

public sealed record ScanHistoryFilter(DateTimeOffset? From = null, DateTimeOffset? To = null, int Limit = 100);
