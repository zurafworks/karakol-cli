namespace Karakol.Persistence.SQLite;

public sealed record SQLitePersistenceOptions(bool Enabled = false, string ConnectionString = "Data Source=karakol.db");
