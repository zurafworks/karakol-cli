using FluentAssertions;
using Karakol.Domain.Enums;
using Karakol.Domain.Findings;
using Karakol.Domain.Risk;
using Karakol.Domain.Scans;
using Karakol.Domain.Sources;
using Karakol.Reporting.Options;
using Karakol.Reporting.Sanitization;
using Karakol.Reporting.Writers.Html;
using Karakol.Reporting.Writers.Json;
using Karakol.Reporting.Writers.Markdown;
using System.Text.Json;

namespace Karakol.Reporting.Tests;

public sealed class ReportingTests
{
    [Theory]
    [InlineData("/login?username=ali&password=123456", "/login?username=ali&password=******")]
    [InlineData("Authorization: Bearer abcdefghijklmnopqrstuvwxyz", "Authorization: Bearer ******")]
    [InlineData("user@example.com", "******@******")]
    public void SensitiveDataMasker_masks_known_secret_patterns(string input, string expected)
    {
        new SensitiveDataMasker().Mask(input).Should().Be(expected);
    }

    [Fact]
    public async Task JsonReportWriter_creates_json_file()
    {
        var directory = CreateTempDirectory();
        var result = await new JsonReportWriter().WriteAsync(Report(), new ReportOptions(directory, true, false), CancellationToken.None);

        File.Exists(result.FilePath).Should().BeTrue();
        result.Format.Should().Be("json");
        (await File.ReadAllTextAsync(result.FilePath)).Should().Contain("Karakol");
    }

    [Fact]
    public async Task JsonReportWriter_uses_stable_report_document_schema()
    {
        var directory = CreateTempDirectory();
        var result = await new JsonReportWriter().WriteAsync(Report(), new ReportOptions(directory, true, false), CancellationToken.None);

        var json = await File.ReadAllTextAsync(result.FilePath);
        json.Should().Contain("\"SchemaVersion\": \"karakol.report.v1\"");
        json.Should().Contain("\"Summary\"");
        json.Should().Contain("\"RiskComponents\"");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        root.GetProperty("SchemaVersion").GetString().Should().Be("karakol.report.v1");
        root.GetProperty("ToolName").GetString().Should().Be("Karakol");
        root.GetProperty("Summary").GetProperty("OverallRisk").GetInt32().Should().Be(80);
        root.GetProperty("Findings").GetArrayLength().Should().Be(1);
        root.GetProperty("Findings")[0].GetProperty("Evidence").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task JsonReportWriter_masks_sensitive_values_when_masking_enabled()
    {
        var directory = CreateTempDirectory();
        var result = await new JsonReportWriter(new SensitiveDataMasker()).WriteAsync(Report("/login?password=secret"), new ReportOptions(directory, true, false), CancellationToken.None);

        var json = await File.ReadAllTextAsync(result.FilePath);
        json.Should().Contain("password=******");
        json.Should().NotContain("password=secret");
    }

    [Fact]
    public async Task MarkdownReportWriter_creates_markdown_file()
    {
        var directory = CreateTempDirectory();
        var result = await new MarkdownReportWriter().WriteAsync(Report(), new ReportOptions(directory, true, false), CancellationToken.None);

        File.Exists(result.FilePath).Should().BeTrue();
        (await File.ReadAllTextAsync(result.FilePath)).Should().Contain("# Karakol Scan Report");
    }

    [Fact]
    public async Task MarkdownReportWriter_includes_summary_and_findings_sections()
    {
        var directory = CreateTempDirectory();
        var result = await new MarkdownReportWriter().WriteAsync(Report(), new ReportOptions(directory, true, false), CancellationToken.None);

        var markdown = await File.ReadAllTextAsync(result.FilePath);
        markdown.Should().Contain("- Overall Risk: **80/100 High**");
        markdown.Should().Contain("| Severity | Count |");
        markdown.Should().Contain("## Findings");
        markdown.Should().Contain("`sqli.basic`");
    }

    [Fact]
    public async Task HtmlReportWriter_encodes_html_payload()
    {
        var directory = CreateTempDirectory();
        var result = await new HtmlReportWriter(new SensitiveDataMasker()).WriteAsync(Report("/search?q=<script>alert(1)</script>"), new ReportOptions(directory, true, false), CancellationToken.None);

        var html = await File.ReadAllTextAsync(result.FilePath);
        html.Should().Contain("&lt;script&gt;");
        html.Should().NotContain("<script>alert(1)</script>");
    }

    [Fact]
    public async Task HtmlReportWriter_masks_sensitive_values()
    {
        var directory = CreateTempDirectory();
        var result = await new HtmlReportWriter(new SensitiveDataMasker()).WriteAsync(Report("/login?password=secret"), new ReportOptions(directory, true, false), CancellationToken.None);

        var html = await File.ReadAllTextAsync(result.FilePath);
        html.Should().Contain("password=******");
        html.Should().NotContain("password=secret");
    }

    [Fact]
    public async Task HtmlReportWriter_includes_dashboard_sections()
    {
        var directory = CreateTempDirectory();
        var result = await new HtmlReportWriter(new SensitiveDataMasker()).WriteAsync(Report(), new ReportOptions(directory, true, false), CancellationToken.None);

        var html = await File.ReadAllTextAsync(result.FilePath);
        html.Should().Contain("data-section=\"executive-summary\"");
        html.Should().Contain("data-section=\"severity-distribution\"");
        html.Should().Contain("data-section=\"threat-category-distribution\"");
        html.Should().Contain("data-section=\"timeline\"");
        html.Should().Contain("data-section=\"critical-findings\"");
        html.Should().Contain("data-section=\"all-findings\"");
        html.Should().Contain("data-section=\"evidence-details\"");
        html.Should().Contain("data-section=\"risk-components\"");
        html.Should().Contain("brand-logo");
        html.Should().Contain("data:image/png;base64");
        html.Should().Contain("Top Source IPs");
        html.Should().Contain("Timeline");
        html.Should().Contain("Risk Components");
        html.Should().Contain("Metadata");
    }

    [Fact]
    public async Task HtmlReportWriter_includes_responsive_print_and_large_table_styles()
    {
        var directory = CreateTempDirectory();
        var result = await new HtmlReportWriter(new SensitiveDataMasker()).WriteAsync(Report(), new ReportOptions(directory, true, false), CancellationToken.None);

        var html = await File.ReadAllTextAsync(result.FilePath);
        html.Should().Contain("@media print");
        html.Should().Contain("@media (max-width: 640px)");
        html.Should().Contain("findings-table");
        html.Should().Contain("position: sticky");
    }

    [Fact]
    public async Task HtmlReportWriter_keeps_empty_states_when_report_has_no_findings()
    {
        var directory = CreateTempDirectory();
        var result = await new HtmlReportWriter(new SensitiveDataMasker()).WriteAsync(EmptyReport(), new ReportOptions(directory, true, false), CancellationToken.None);

        var html = await File.ReadAllTextAsync(result.FilePath);
        html.Should().Contain("No findings matched the selected filters.");
        html.Should().Contain("No high or critical findings.");
        html.Should().Contain("No evidence records.");
        html.Should().Contain("No risk component records.");
        html.Should().Contain("No timeline buckets available.");
    }

    [Fact]
    public async Task HtmlReportWriter_uses_text_logo_fallback_when_logo_file_is_not_available()
    {
        var originalDirectory = Directory.GetCurrentDirectory();
        var isolatedDirectory = CreateTempDirectory();

        try
        {
            Directory.SetCurrentDirectory(isolatedDirectory);
            var directory = CreateTempDirectory();
            var result = await new HtmlReportWriter(new SensitiveDataMasker()).WriteAsync(Report(), new ReportOptions(directory, true, false), CancellationToken.None);

            var html = await File.ReadAllTextAsync(result.FilePath);
            html.Should().Contain("brand-fallback");
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    private static ScanReport Report(string target = "/login?id=1")
    {
        var finding = new DetectionFinding(ThreatCategory.SqlInjection, Severity.High, 0.9, "sqli.basic", "SQLi", DetectionType.Rule, "SQL Injection", "Description", "Reason", [new Evidence("Url", target, "union select", "matched")], "Fix")
        {
            SourceIp = "1.2.3.4",
            TargetResource = target,
            RiskScore = new RiskScore(80)
        };
        var summary = new ScanSummary(1, 1, 0, 1, 1, new RiskScore(80), new Dictionary<ThreatCategory, int> { [ThreatCategory.SqlInjection] = 1 }, new Dictionary<Severity, int> { [Severity.High] = 1 }, [], [], [], [], ["Fix it"]);

        return new ScanReport(
            ScanId.New(),
            "Karakol",
            "0.1.0",
            DateTimeOffset.UtcNow,
            new LogSource(LogSourceId.New(), "sample.log", LogFormat.NginxAccess, 1, null, null, LogSourceType.File),
            summary,
            [finding],
            [],
            summary.Recommendations,
            new ScanConfigurationSnapshot(LogFormat.NginxAccess, ["json"], true, false, true, true, null),
            new Dictionary<string, string>());
    }

    private static ScanReport EmptyReport()
    {
        var summary = new ScanSummary(0, 0, 0, 0, 0, new RiskScore(0), new Dictionary<ThreatCategory, int>(), new Dictionary<Severity, int>(), [], [], [], [], []);

        return new ScanReport(
            ScanId.New(),
            "Karakol",
            "0.1.0",
            DateTimeOffset.UtcNow,
            new LogSource(LogSourceId.New(), "empty.log", LogFormat.NginxAccess, 0, null, null, LogSourceType.File),
            summary,
            [],
            [],
            [],
            new ScanConfigurationSnapshot(LogFormat.NginxAccess, ["html"], true, false, true, true, null),
            new Dictionary<string, string>());
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "karakol-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
