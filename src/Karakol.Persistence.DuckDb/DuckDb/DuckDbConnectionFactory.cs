using DuckDB.NET.Data;

namespace Karakol.Persistence.DuckDb.DuckDb;

public sealed class DuckDbConnectionFactory(DuckDbPersistenceOptions options)
{
    public DuckDBConnection CreateOpenConnection()
    {
        if (!options.Enabled)
        {
            throw new InvalidOperationException("DuckDB persistence is disabled. Enable it explicitly with provider 'DuckDB'.");
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(options.DatabasePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var connection = new DuckDBConnection(options.ConnectionString);
        connection.Open();
        return connection;
    }
}
