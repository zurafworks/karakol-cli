using Karakol.Domain.Scans;
using Karakol.Domain.Enums;
using Karakol.Persistence.Abstractions;
using Karakol.Persistence.DuckDb.DuckDb;
using Karakol.Persistence.DuckDb.Sanitization;

namespace Karakol.Persistence.DuckDb.Repository;

public sealed class DuckDbScanHistoryRepository(DuckDbConnectionFactory connectionFactory) : IScanHistoryRepository
{
    public Task SaveAsync(ScanReport report, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(report);
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = connectionFactory.CreateOpenConnection();
        DeleteById(connection, report.ScanId);

        connection.ExecuteNonQuery(
            """
            INSERT INTO scan_sessions (
                scan_id, generated_at, tool_name, tool_version, source_path, source_format, source_size_bytes,
                total_lines, parsed_events, failed_lines, findings_count, suspicious_events,
                overall_risk, overall_severity
            )
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);
            """,
            report.ScanId.Value.ToString("D"),
            report.GeneratedAt.UtcDateTime,
            report.ToolName,
            report.ToolVersion,
            report.Source.FilePath,
            report.Source.Format.ToString(),
            report.Source.SizeInBytes,
            report.Summary.TotalLines,
            report.Summary.ParsedEvents,
            report.Summary.FailedLines,
            report.Summary.FindingsCount,
            report.Summary.SuspiciousEvents,
            report.Summary.OverallRisk.Value,
            report.Summary.OverallRisk.Severity.ToString());

        foreach (var finding in report.Findings)
        {
            cancellationToken.ThrowIfCancellationRequested();
            connection.ExecuteNonQuery(
                """
                INSERT INTO findings (
                    finding_id, scan_id, category, severity, risk_score, confidence, detector_id,
                    detector_name, detection_type, title, reason, recommended_action,
                    source_ip, target_resource, created_at
                )
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);
                """,
                finding.Id.Value.ToString("D"),
                report.ScanId.Value.ToString("D"),
                finding.Category.ToString(),
                finding.Severity.ToString(),
                finding.RiskScore.Value,
                finding.Confidence,
                finding.DetectorId,
                finding.DetectorName,
                finding.DetectionType.ToString(),
                finding.Title,
                PersistenceValueSanitizer.Mask(finding.Reason),
                PersistenceValueSanitizer.Mask(finding.RecommendedAction),
                finding.SourceIp,
                PersistenceValueSanitizer.Mask(finding.TargetResource),
                finding.CreatedAt.UtcDateTime);

            foreach (var evidence in finding.Evidence)
            {
                connection.ExecuteNonQuery(
                    """
                    INSERT INTO finding_evidence (finding_id, field, value, matched_pattern, explanation)
                    VALUES (?, ?, ?, ?, ?);
                    """,
                    finding.Id.Value.ToString("D"),
                    evidence.Field,
                    PersistenceValueSanitizer.Mask(evidence.Value),
                    PersistenceValueSanitizer.Mask(evidence.MatchedPattern),
                    evidence.Explanation);
            }
        }

        foreach (var bucket in report.Timeline)
        {
            connection.ExecuteNonQuery(
                """
                INSERT INTO timeline_buckets (scan_id, starts_at, event_count, finding_count)
                VALUES (?, ?, ?, ?);
                """,
                report.ScanId.Value.ToString("D"),
                bucket.StartsAt?.UtcDateTime,
                bucket.EventCount,
                bucket.FindingCount);
        }

        return Task.CompletedTask;
    }

    public Task<ScanReport?> GetByIdAsync(ScanId scanId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<ScanReport?>(null);
    }

    public Task<IReadOnlyList<ScanHistoryItem>> ListAsync(ScanHistoryFilter filter, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var connection = connectionFactory.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT scan_id, generated_at, source_path, overall_risk, overall_severity
            FROM scan_sessions
            ORDER BY generated_at DESC
            LIMIT 100;
            """;

        using var reader = command.ExecuteReader();
        var items = new List<ScanHistoryItem>();
        while (reader.Read())
        {
            items.Add(new ScanHistoryItem(
                new ScanId(Guid.Parse(reader.GetString(0))),
                reader.GetString(2),
                new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime(1), DateTimeKind.Utc)),
                reader.GetInt32(3),
                Enum.Parse<Severity>(reader.GetString(4))));
        }

        return Task.FromResult<IReadOnlyList<ScanHistoryItem>>(items);
    }

    public Task DeleteAsync(ScanId scanId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var connection = connectionFactory.CreateOpenConnection();
        DeleteById(connection, scanId);
        return Task.CompletedTask;
    }

    private static void DeleteById(System.Data.Common.DbConnection connection, ScanId scanId)
    {
        var id = scanId.Value.ToString("D");
        connection.ExecuteNonQuery("DELETE FROM finding_evidence WHERE finding_id IN (SELECT finding_id FROM findings WHERE scan_id = ?);", id);
        connection.ExecuteNonQuery("DELETE FROM findings WHERE scan_id = ?;", id);
        connection.ExecuteNonQuery("DELETE FROM timeline_buckets WHERE scan_id = ?;", id);
        connection.ExecuteNonQuery("DELETE FROM scan_sessions WHERE scan_id = ?;", id);
    }
}
