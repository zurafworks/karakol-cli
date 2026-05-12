using Karakol.Application.Commands.ScanLogFile;
using Karakol.Domain.Enums;
using Spectre.Console;

namespace Karakol.Cli.Rendering;

public static class ScanSummaryRenderer
{
    public static void Render(ScanLogFileResult result, bool verbose = false)
    {
        var dto = result.Dto;
        var summary = new Table().NoBorder();
        summary.AddColumn("Metric");
        summary.AddColumn("Value");
        summary.AddRow("Source", dto.Source);
        summary.AddRow("Detected Format", dto.DetectedFormat);
        summary.AddRow("Lines Analyzed", dto.TotalLines.ToString());
        summary.AddRow("Parsed Events", dto.ParsedEvents.ToString());
        summary.AddRow("Failed Lines", dto.FailedLines.ToString());
        summary.AddRow("Overall Risk", $"{dto.RiskScore}/100 {dto.RiskSeverity}");
        AnsiConsole.Write(new Panel(summary).Header("Karakol"));

        var findings = new Table().RoundedBorder();
        findings.AddColumn("Severity");
        findings.AddColumn("Count");
        foreach (var severity in Enum.GetValues<Severity>().Reverse())
        {
            var count = result.Report.Summary.SeverityCounts.GetValueOrDefault(severity);
            findings.AddRow(severity.ToString(), count.ToString());
        }

        AnsiConsole.Write(findings);

        var categories = new Table().RoundedBorder();
        categories.AddColumn("Category");
        categories.AddColumn("Count");
        foreach (var pair in result.Report.Summary.CategoryCounts.OrderByDescending(pair => pair.Value).Take(8))
        {
            categories.AddRow(pair.Key.ToString(), pair.Value.ToString());
        }

        AnsiConsole.Write(categories);

        var reports = new Table().RoundedBorder();
        reports.AddColumn("Format");
        reports.AddColumn("Path");
        foreach (var report in dto.Reports)
        {
            reports.AddRow(report.Format.ToUpperInvariant(), report.FilePath);
        }

        AnsiConsole.Write(reports);

        if (!verbose)
        {
            return;
        }

        var metadata = new Table().RoundedBorder();
        metadata.AddColumn("Metadata");
        metadata.AddColumn("Value");
        foreach (var item in result.Report.Metadata.OrderBy(item => item.Key))
        {
            metadata.AddRow(item.Key, item.Value);
        }

        AnsiConsole.Write(metadata);
    }
}
