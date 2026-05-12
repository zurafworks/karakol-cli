using Karakol.Persistence.DuckDb.DuckDb;

namespace Karakol.Persistence.DuckDb.Analytics;

public sealed class DuckDbAnalyticsRepository(DuckDbConnectionFactory connectionFactory)
{
    public IReadOnlyList<SourceIpFindingCount> GetTopSourceIps(int limit = 10)
    {
        using var connection = connectionFactory.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT source_ip, count(*)::INTEGER AS finding_count
            FROM findings
            WHERE source_ip IS NOT NULL AND source_ip <> ''
            GROUP BY source_ip
            ORDER BY finding_count DESC, source_ip
            LIMIT ?;
            """;
        var parameter = command.CreateParameter();
        parameter.Value = limit;
        command.Parameters.Add(parameter);

        using var reader = command.ExecuteReader();
        var results = new List<SourceIpFindingCount>();
        while (reader.Read())
        {
            results.Add(new SourceIpFindingCount(reader.GetString(0), reader.GetInt32(1)));
        }

        return results;
    }

    public IReadOnlyList<ThreatCategoryTrend> GetCategoryTrend()
    {
        using var connection = connectionFactory.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT category, CAST(date_trunc('day', created_at) AS DATE) AS day, count(*)::INTEGER AS finding_count
            FROM findings
            GROUP BY category, day
            ORDER BY day, category;
            """;

        using var reader = command.ExecuteReader();
        var results = new List<ThreatCategoryTrend>();
        while (reader.Read())
        {
            results.Add(new ThreatCategoryTrend(reader.GetString(0), DateOnly.FromDateTime(reader.GetDateTime(1)), reader.GetInt32(2)));
        }

        return results;
    }

    public IReadOnlyList<SeverityTrend> GetSeverityTrend()
    {
        using var connection = connectionFactory.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT severity, CAST(date_trunc('day', created_at) AS DATE) AS day, count(*)::INTEGER AS finding_count
            FROM findings
            GROUP BY severity, day
            ORDER BY day, severity;
            """;

        using var reader = command.ExecuteReader();
        var results = new List<SeverityTrend>();
        while (reader.Read())
        {
            results.Add(new SeverityTrend(reader.GetString(0), DateOnly.FromDateTime(reader.GetDateTime(1)), reader.GetInt32(2)));
        }

        return results;
    }

    public IReadOnlyList<ParserFailureRatio> GetParserFailureRatios()
    {
        using var connection = connectionFactory.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT scan_id, source_path,
                CASE WHEN total_lines = 0 THEN 0 ELSE CAST(failed_lines AS DOUBLE) / CAST(total_lines AS DOUBLE) END AS ratio
            FROM scan_sessions
            ORDER BY generated_at DESC;
            """;

        using var reader = command.ExecuteReader();
        var results = new List<ParserFailureRatio>();
        while (reader.Read())
        {
            results.Add(new ParserFailureRatio(reader.GetString(0), reader.GetString(1), reader.GetDouble(2)));
        }

        return results;
    }

    public IReadOnlyList<SensitivePathAccess> GetRepeatedSensitivePathAccess(int minimumCount = 2)
    {
        using var connection = connectionFactory.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT target_resource, count(*)::INTEGER AS finding_count
            FROM findings
            WHERE category = 'SensitiveFileAccess' AND target_resource IS NOT NULL AND target_resource <> ''
            GROUP BY target_resource
            HAVING finding_count >= ?
            ORDER BY finding_count DESC, target_resource;
            """;
        var parameter = command.CreateParameter();
        parameter.Value = minimumCount;
        command.Parameters.Add(parameter);

        using var reader = command.ExecuteReader();
        var results = new List<SensitivePathAccess>();
        while (reader.Read())
        {
            results.Add(new SensitivePathAccess(reader.GetString(0), reader.GetInt32(1)));
        }

        return results;
    }
}
