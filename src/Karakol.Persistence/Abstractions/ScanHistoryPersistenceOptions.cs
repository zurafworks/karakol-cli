namespace Karakol.Persistence.Abstractions;

public sealed record ScanHistoryPersistenceOptions(
    bool Enabled,
    string Provider,
    string? ConnectionString);
