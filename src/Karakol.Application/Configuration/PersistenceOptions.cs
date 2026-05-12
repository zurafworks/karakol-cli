namespace Karakol.Application.Configuration;

public sealed class PersistenceOptions
{
    public bool Enabled { get; init; }
    public string Provider { get; init; } = "SQLite";
    public string? ConnectionString { get; init; }
}
