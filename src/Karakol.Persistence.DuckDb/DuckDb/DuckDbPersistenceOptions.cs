namespace Karakol.Persistence.DuckDb.DuckDb;

public sealed class DuckDbPersistenceOptions
{
    public bool Enabled { get; init; }

    public string DatabasePath { get; init; } = "karakol.duckdb";

    public string ConnectionString => $"Data Source={DatabasePath}";
}
