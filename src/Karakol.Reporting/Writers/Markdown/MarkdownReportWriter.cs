using System.Text;
using Karakol.Domain.Scans;
using Karakol.Reporting.Abstractions;
using Karakol.Reporting.Formats;
using Karakol.Reporting.Options;
using Karakol.Reporting.Results;

namespace Karakol.Reporting.Writers.Markdown;

public sealed class MarkdownReportWriter : IReportWriter
{
    public string Format => ReportFormatNames.Markdown;
    public string FileExtension => ".md";

    public async Task<ReportWriteResult> WriteAsync(ScanReport report, ReportOptions options, CancellationToken cancellationToken)
    {
        var outputDirectory = NormalizeOutputDirectory(options.OutputDirectory);
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, $"karakol-report-{report.ScanId.Value:N}.md");
        var builder = new StringBuilder();
        builder.AppendLine("# Karakol Scan Report");
        builder.AppendLine();
        builder.AppendLine($"- Source: `{report.Source.FilePath}`");
        builder.AppendLine($"- Overall Risk: **{report.Summary.OverallRisk.Value}/100 {report.Summary.OverallRisk.Severity}**");
        builder.AppendLine($"- Parsed Events: {report.Summary.ParsedEvents}");
        builder.AppendLine($"- Findings: {report.Summary.FindingsCount}");
        builder.AppendLine();
        builder.AppendLine("| Severity | Count |");
        builder.AppendLine("| --- | ---: |");
        foreach (var item in report.Summary.SeverityCounts.OrderByDescending(pair => pair.Key))
        {
            builder.AppendLine($"| {item.Key} | {item.Value} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Findings");
        foreach (var finding in report.Findings.OrderByDescending(finding => finding.RiskScore.Value))
        {
            builder.AppendLine($"- **{finding.Severity}** `{finding.DetectorId}` {finding.Title} from `{finding.SourceIp ?? "unknown"}`: {finding.Reason}");
        }

        await File.WriteAllTextAsync(path, builder.ToString(), cancellationToken).ConfigureAwait(false);
        var fileInfo = new FileInfo(path);
        return new ReportWriteResult(path, Format, fileInfo.Length);
    }

    private static string NormalizeOutputDirectory(string outputDirectory)
    {
        var fullPath = Path.GetFullPath(outputDirectory);
        if (File.Exists(fullPath))
        {
            throw new IOException($"Output path '{outputDirectory}' is an existing file.");
        }

        return fullPath;
    }
}
