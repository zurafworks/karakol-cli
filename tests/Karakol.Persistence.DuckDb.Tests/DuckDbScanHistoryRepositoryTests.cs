using FluentAssertions;
using Karakol.Domain.Enums;
using Karakol.Domain.Findings;
using Karakol.Domain.Risk;
using Karakol.Domain.Scans;
using Karakol.Domain.Sources;
using Karakol.Domain.ValueObjects;
using Karakol.Persistence.DuckDb.Analytics;
using Karakol.Persistence.DuckDb.DuckDb;
using Karakol.Persistence.DuckDb.Repository;
using Karakol.Persistence.DuckDb.Schema;

namespace Karakol.Persistence.DuckDb.Tests;

public sealed class DuckDbScanHistoryRepositoryTests
{
    [Fact]
    public async Task SaveAsync_PersistsDerivedScanFactsWithoutRawLogStorage()
    {
        using var database = new TemporaryDuckDb();
        var report = CreateReport("/login?password=123456");
        var repository = database.CreateRepository();

        await repository.SaveAsync(report, CancellationToken.None);

        var history = await repository.ListAsync(new(), CancellationToken.None);

        history.Should().ContainSingle();
        history[0].ScanId.Should().Be(report.ScanId);
        history[0].RiskScore.Should().Be(80);
        database.TableExists("raw_logs").Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_MasksSensitiveEvidenceValues()
    {
        using var database = new TemporaryDuckDb();
        var report = CreateReport("/login?password=123456");

        await database.CreateRepository().SaveAsync(report, CancellationToken.None);

        var storedEvidenceValue = database.ExecuteScalar<string>(
            "SELECT value FROM finding_evidence WHERE field = 'Url' LIMIT 1;");

        storedEvidenceValue.Should().Contain("password=******");
        storedEvidenceValue.Should().NotContain("123456");
    }

    [Fact]
    public async Task AnalyticsQueries_ReturnExpectedAggregates()
    {
        using var database = new TemporaryDuckDb();
        await database.CreateRepository().SaveAsync(CreateReport("/.env"), CancellationToken.None);

        var analytics = database.CreateAnalyticsRepository();

        analytics.GetTopSourceIps().Should().ContainSingle(item => item.SourceIp == "203.0.113.10" && item.FindingCount == 1);
        analytics.GetCategoryTrend().Should().ContainSingle(item => item.Category == ThreatCategory.SensitiveFileAccess.ToString());
        analytics.GetSeverityTrend().Should().ContainSingle(item => item.Severity == Severity.High.ToString());
        analytics.GetParserFailureRatios().Should().ContainSingle(item => item.Ratio == 0.1);
        analytics.GetRepeatedSensitivePathAccess(1).Should().ContainSingle(item => item.TargetResource == "/.env");
    }

    [Fact]
    public async Task DisabledOptions_PreventDatabaseCreation()
    {
        var options = new DuckDbPersistenceOptions { Enabled = false, DatabasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.duckdb") };
        var repository = new DuckDbScanHistoryRepository(new DuckDbConnectionFactory(options));

        var act = async () => await repository.ListAsync(new(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        File.Exists(options.DatabasePath).Should().BeFalse();
    }

    private static ScanReport CreateReport(string target)
    {
        var finding = new DetectionFinding(
            ThreatCategory.SensitiveFileAccess,
            Severity.High,
            0.95,
            "sensitive-file.basic",
            "Sensitive File Access",
            DetectionType.Rule,
            "Sensitive file access",
            "Sensitive path requested",
            $"Target resource {target}",
            [new Evidence("Url", target, ".env", "Sensitive file request detected.")],
            "Review exposed files.")
        {
            RiskScore = new RiskScore(80),
            SourceIp = "203.0.113.10",
            TargetResource = target,
            CreatedAt = new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero)
        };

        var summary = new ScanSummary(
            TotalLines: 10,
            ParsedEvents: 9,
            FailedLines: 1,
            FindingsCount: 1,
            SuspiciousEvents: 1,
            OverallRisk: new RiskScore(80),
            CategoryCounts: new Dictionary<ThreatCategory, int> { [ThreatCategory.SensitiveFileAccess] = 1 },
            SeverityCounts: new Dictionary<Severity, int> { [Severity.High] = 1 },
            TopSourceIps: [new TopSourceIpSummary("203.0.113.10", 1)],
            TopTargetUrls: [new TopTargetUrlSummary(target, 1)],
            TopUserAgents: [],
            Timeline: [new TimelineBucket(new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero), 9, 1)],
            Recommendations: ["Review sensitive file exposure."]);

        return new ScanReport(
            ScanId.New(),
            "Karakol",
            "0.1.0",
            new DateTimeOffset(2026, 5, 10, 12, 0, 0, TimeSpan.Zero),
            new LogSource(LogSourceId.New(), "samples/nginx/nginx-access.log", LogFormat.NginxAccess, 1024, null, null, LogSourceType.File),
            summary,
            [finding],
            summary.Timeline,
            summary.Recommendations,
            new ScanConfigurationSnapshot(LogFormat.NginxAccess, ["json"], true, false, true, true, null),
            new Dictionary<string, string>());
    }

    private sealed class TemporaryDuckDb : IDisposable
    {
        private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.duckdb");

        public TemporaryDuckDb()
        {
            var initializer = new DuckDbSchemaInitializer(CreateFactory());
            initializer.Initialize();
        }

        public DuckDbScanHistoryRepository CreateRepository() => new(CreateFactory());

        public DuckDbAnalyticsRepository CreateAnalyticsRepository() => new(CreateFactory());

        public bool TableExists(string tableName)
        {
            var count = ExecuteScalar<long>("SELECT count(*) FROM information_schema.tables WHERE table_name = ?;", tableName);
            return count > 0;
        }

        public T ExecuteScalar<T>(string sql, params object?[] parameters)
        {
            using var connection = CreateFactory().CreateOpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            foreach (var value in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.Value = value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            return (T)command.ExecuteScalar()!;
        }

        public void Dispose()
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }

        private DuckDbConnectionFactory CreateFactory()
        {
            return new DuckDbConnectionFactory(new DuckDbPersistenceOptions { Enabled = true, DatabasePath = databasePath });
        }
    }
}
