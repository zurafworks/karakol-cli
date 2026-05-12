using Karakol.Persistence.Abstractions;
using Karakol.Persistence.DuckDb.DuckDb;
using Karakol.Persistence.DuckDb.Schema;

namespace Karakol.Persistence.DuckDb.Repository;

public sealed class DuckDbScanHistoryRepositoryProvider : IScanHistoryRepositoryProvider
{
    public string ProviderName => "DuckDB";

    public bool CanCreate(ScanHistoryPersistenceOptions options)
    {
        return options.Enabled && string.Equals(options.Provider, ProviderName, StringComparison.OrdinalIgnoreCase);
    }

    public IScanHistoryRepository Create(ScanHistoryPersistenceOptions options)
    {
        if (!CanCreate(options))
        {
            throw new InvalidOperationException("DuckDB scan history provider cannot create a repository for the supplied options.");
        }

        var databasePath = ResolveDatabasePath(options.ConnectionString);
        var duckDbOptions = new DuckDbPersistenceOptions { Enabled = true, DatabasePath = databasePath };
        var connectionFactory = new DuckDbConnectionFactory(duckDbOptions);
        new DuckDbSchemaInitializer(connectionFactory).Initialize();
        return new DuckDbScanHistoryRepository(connectionFactory);
    }

    private static string ResolveDatabasePath(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return "karakol.duckdb";
        }

        const string dataSourcePrefix = "Data Source=";
        if (connectionString.StartsWith(dataSourcePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return connectionString[dataSourcePrefix.Length..].Trim();
        }

        return connectionString;
    }
}
