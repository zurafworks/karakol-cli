using Karakol.Persistence.DuckDb.DuckDb;

namespace Karakol.Persistence.DuckDb.Schema;

public sealed class DuckDbSchemaInitializer(DuckDbConnectionFactory connectionFactory)
{
    public void Initialize()
    {
        using var connection = connectionFactory.CreateOpenConnection();

        connection.ExecuteNonQuery(
            """
            CREATE TABLE IF NOT EXISTS schema_version (
                version INTEGER PRIMARY KEY,
                applied_at TIMESTAMP NOT NULL
            );
            """);

        connection.ExecuteNonQuery(
            """
            INSERT INTO schema_version
            SELECT 1, current_timestamp
            WHERE NOT EXISTS (SELECT 1 FROM schema_version WHERE version = 1);
            """);

        connection.ExecuteNonQuery(
            """
            CREATE TABLE IF NOT EXISTS scan_sessions (
                scan_id VARCHAR PRIMARY KEY,
                generated_at TIMESTAMP NOT NULL,
                tool_name VARCHAR NOT NULL,
                tool_version VARCHAR NOT NULL,
                source_path VARCHAR NOT NULL,
                source_format VARCHAR NOT NULL,
                source_size_bytes BIGINT NOT NULL,
                total_lines INTEGER NOT NULL,
                parsed_events INTEGER NOT NULL,
                failed_lines INTEGER NOT NULL,
                findings_count INTEGER NOT NULL,
                suspicious_events INTEGER NOT NULL,
                overall_risk INTEGER NOT NULL,
                overall_severity VARCHAR NOT NULL
            );
            """);

        connection.ExecuteNonQuery(
            """
            CREATE TABLE IF NOT EXISTS findings (
                finding_id VARCHAR PRIMARY KEY,
                scan_id VARCHAR NOT NULL,
                category VARCHAR NOT NULL,
                severity VARCHAR NOT NULL,
                risk_score INTEGER NOT NULL,
                confidence DOUBLE NOT NULL,
                detector_id VARCHAR NOT NULL,
                detector_name VARCHAR NOT NULL,
                detection_type VARCHAR NOT NULL,
                title VARCHAR NOT NULL,
                reason VARCHAR NOT NULL,
                recommended_action VARCHAR NOT NULL,
                source_ip VARCHAR,
                target_resource VARCHAR,
                created_at TIMESTAMP NOT NULL
            );
            """);

        connection.ExecuteNonQuery(
            """
            CREATE TABLE IF NOT EXISTS finding_evidence (
                finding_id VARCHAR NOT NULL,
                field VARCHAR NOT NULL,
                value VARCHAR,
                matched_pattern VARCHAR,
                explanation VARCHAR NOT NULL
            );
            """);

        connection.ExecuteNonQuery(
            """
            CREATE TABLE IF NOT EXISTS timeline_buckets (
                scan_id VARCHAR NOT NULL,
                starts_at TIMESTAMP,
                event_count INTEGER NOT NULL,
                finding_count INTEGER NOT NULL
            );
            """);
    }
}
