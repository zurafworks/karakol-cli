namespace Karakol.Persistence.History.Queries;

public sealed record GetScanHistoryQuery(DateTimeOffset? From = null, DateTimeOffset? To = null, int Limit = 100);
