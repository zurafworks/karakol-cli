using System.Net;
using Karakol.Domain.Enums;
using Karakol.Domain.Scans;
using Karakol.Reporting.Abstractions;
using Karakol.Reporting.Formats;
using Karakol.Reporting.Options;
using Karakol.Reporting.Results;

namespace Karakol.Reporting.Writers.Html;

public sealed class HtmlReportWriter(ISensitiveDataMasker masker) : IReportWriter
{
    public string Format => ReportFormatNames.Html;
    public string FileExtension => ".html";

    public async Task<ReportWriteResult> WriteAsync(ScanReport report, ReportOptions options, CancellationToken cancellationToken)
    {
        var outputDirectory = NormalizeOutputDirectory(options.OutputDirectory);
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, $"karakol-report-{report.ScanId.Value:N}.html");
        await File.WriteAllTextAsync(path, BuildHtml(report, options), cancellationToken).ConfigureAwait(false);
        var fileInfo = new FileInfo(path);
        return new ReportWriteResult(path, Format, fileInfo.Length);
    }

    private string BuildHtml(ScanReport report, ReportOptions options)
    {
        var findings = report.Findings.OrderByDescending(finding => finding.RiskScore.Value).ToArray();
        var criticalFindings = findings.Where(finding => finding.Severity is Severity.Critical or Severity.High).Take(10).ToArray();
        var logoDataUri = TryLoadLogoDataUri();
        var logoMarkup = logoDataUri is null
            ? """<div class="brand-fallback">K</div>"""
            : $"""<img class="brand-logo" src="{logoDataUri}" alt="Karakol logo">""";

        var rows = string.Join(Environment.NewLine, findings.Select(finding =>
            "<tr>" +
            $"<td>{SeverityBadge(finding.Severity)}</td>" +
            $"<td>{Encode(finding.Category.ToString())}</td>" +
            $"<td>{RiskMeter(finding.RiskScore.Value)}</td>" +
            $"<td>{Encode(finding.SourceIp ?? "unknown")}</td>" +
            $"<td>{Encode(options.MaskSensitiveData ? masker.Mask(finding.TargetResource ?? "") : finding.TargetResource ?? "")}</td>" +
            $"<td>{Encode(finding.Title)}</td>" +
            $"<td>{Encode(finding.Reason)}</td>" +
            "</tr>"));

        var criticalRows = string.Join(Environment.NewLine, criticalFindings.Select(finding =>
            "<tr>" +
            $"<td>{SeverityBadge(finding.Severity)}</td>" +
            $"<td>{Encode(finding.Category.ToString())}</td>" +
            $"<td>{RiskMeter(finding.RiskScore.Value)}</td>" +
            $"<td>{Encode(options.MaskSensitiveData ? masker.Mask(finding.TargetResource ?? "") : finding.TargetResource ?? "")}</td>" +
            $"<td>{Encode(finding.RecommendedAction)}</td>" +
            "</tr>"));

        var riskComponents = string.Join(Environment.NewLine, findings.SelectMany(finding => finding.RiskScore.Components.Select(component =>
            "<tr>" +
            $"<td>{Encode(finding.DetectorId)}</td>" +
            $"<td>{Encode(component.Name)}</td>" +
            $"<td>{ComponentValue(component.Contribution)}</td>" +
            $"<td>{Encode(component.Explanation)}</td>" +
            "</tr>")));

        var evidence = string.Join(Environment.NewLine, findings.SelectMany(finding => finding.Evidence.Select(item =>
            "<tr>" +
            $"<td>{Encode(finding.DetectorId)}</td>" +
            $"<td>{Encode(item.Field)}</td>" +
            $"<td>{Encode(options.MaskSensitiveData ? masker.Mask(item.Value ?? "") : item.Value ?? "")}</td>" +
            $"<td>{Encode(item.MatchedPattern ?? "")}</td>" +
            $"<td>{Encode(item.Explanation)}</td>" +
            "</tr>")));

        var categories = DistributionBars(report.Summary.CategoryCounts.OrderByDescending(pair => pair.Value).Select(pair => (pair.Key.ToString(), pair.Value)));
        var severities = SeverityDistribution(report.Summary.SeverityCounts);
        var recommendations = ToActionList(report.Recommendations);
        var topIps = ToRankedList(report.Summary.TopSourceIps.Select(item => (item.SourceIp, $"{item.SuspiciousEvents} suspicious events")));
        var topUrls = ToRankedList(report.Summary.TopTargetUrls.Select(item => (options.MaskSensitiveData ? masker.Mask(item.Url) : item.Url, $"{item.Count} hits")));
        var topUserAgents = ToRankedList(report.Summary.TopUserAgents.Select(item => (options.MaskSensitiveData ? masker.Mask(item.UserAgent) : item.UserAgent, $"{item.Count} hits")));
        var timeline = string.Join(Environment.NewLine, report.Summary.Timeline.Select(item =>
            "<tr>" +
            $"<td>{Encode(item.StartsAt?.ToString("u") ?? "unknown")}</td>" +
            $"<td>{item.EventCount}</td>" +
            $"<td>{item.FindingCount}</td>" +
            "</tr>"));
        var metadata = ToKeyValueList(report.Metadata.Select(pair => (pair.Key, pair.Value)));
        var parserQuality = report.Summary.TotalLines == 0
            ? 100
            : Math.Clamp((int)Math.Round(report.Summary.ParsedEvents * 100.0 / report.Summary.TotalLines), 0, 100);
        var generatedAt = report.GeneratedAt.ToString("u");
        var externalApiState = report.Metadata.TryGetValue("externalApiCalls", out var externalApiCalls)
            ? externalApiCalls
            : "false";
        var emptyFindingRows = """<tr><td colspan="7" class="empty">No findings matched the selected filters.</td></tr>""";
        var emptyCriticalRows = """<tr><td colspan="5" class="empty">No high or critical findings.</td></tr>""";
        var emptyEvidenceRows = """<tr><td colspan="5" class="empty">No evidence records.</td></tr>""";
        var emptyRiskRows = """<tr><td colspan="4" class="empty">No risk component records.</td></tr>""";
        var emptyTimelineRows = """<tr><td colspan="3" class="empty">No timeline buckets available.</td></tr>""";

        return $$"""
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Karakol Scan Report</title>
  <style>
    :root {
      --bg: #f4f6f8;
      --surface: #ffffff;
      --surface-2: #f8fafc;
      --line: #d8dee7;
      --text: #111827;
      --muted: #667085;
      --brand: #15243a;
      --brand-2: #1f6f78;
      --info: #2563eb;
      --low: #168a51;
      --medium: #b7791f;
      --high: #c2410c;
      --critical: #b91c1c;
      --focus: #0f766e;
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      color: var(--text);
      background: var(--bg);
      font-family: "Segoe UI", Arial, sans-serif;
      font-size: 14px;
      line-height: 1.45;
    }
    header {
      background: var(--brand);
      color: #fff;
      border-bottom: 4px solid var(--brand-2);
    }
    main { max-width: 1440px; margin: 0 auto; padding: 24px; }
    section { margin-bottom: 20px; }
    h1, h2, h3, p { margin-top: 0; }
    h1 { font-size: 26px; margin-bottom: 6px; letter-spacing: 0; }
    h2 { font-size: 16px; margin-bottom: 12px; }
    code {
      background: rgba(17, 24, 39, 0.08);
      padding: 2px 6px;
      border-radius: 4px;
      word-break: break-word;
    }
    .header-inner {
      max-width: 1440px;
      margin: 0 auto;
      padding: 24px;
      display: grid;
      grid-template-columns: auto 1fr auto;
      gap: 18px;
      align-items: center;
    }
    .brand-logo {
      width: 72px;
      height: 72px;
      object-fit: contain;
      background: #fff;
      border-radius: 8px;
      padding: 6px;
    }
    .brand-fallback {
      width: 72px;
      height: 72px;
      display: grid;
      place-items: center;
      background: #fff;
      color: var(--brand);
      border-radius: 8px;
      font-weight: 800;
      font-size: 34px;
    }
    .header-meta {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
      justify-content: flex-end;
    }
    .pill {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 5px 9px;
      border-radius: 999px;
      background: rgba(255, 255, 255, 0.12);
      border: 1px solid rgba(255, 255, 255, 0.22);
      color: #fff;
      font-size: 12px;
      white-space: nowrap;
    }
    .grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; }
    .columns { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 12px; }
    .two-columns { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }
    .panel, .metric {
      background: var(--surface);
      border: 1px solid var(--line);
      border-radius: 8px;
      box-shadow: 0 1px 2px rgba(16, 24, 40, 0.04);
    }
    .panel { padding: 16px; }
    .metric { padding: 16px; min-height: 112px; }
    .metric-label { color: var(--muted); font-size: 12px; text-transform: uppercase; }
    .metric strong { display: block; font-size: 26px; margin-top: 8px; }
    .metric small { color: var(--muted); display: block; margin-top: 4px; }
    .risk-hero { border-left: 6px solid {{SeverityColor(report.Summary.OverallRisk.Severity)}}; }
    .risk-ring {
      width: 112px;
      height: 112px;
      border-radius: 50%;
      display: grid;
      place-items: center;
      background: conic-gradient({{SeverityColor(report.Summary.OverallRisk.Severity)}} {{report.Summary.OverallRisk.Value}}%, #e5e7eb 0);
      margin-left: auto;
    }
    .risk-ring span {
      width: 82px;
      height: 82px;
      border-radius: 50%;
      display: grid;
      place-items: center;
      background: #fff;
      font-weight: 800;
      font-size: 20px;
    }
    .summary-layout {
      display: grid;
      grid-template-columns: 1fr auto;
      gap: 18px;
      align-items: center;
    }
    .muted { color: var(--muted); }
    .dist-list, .rank-list, .action-list, .kv-list {
      list-style: none;
      margin: 0;
      padding: 0;
      display: grid;
      gap: 8px;
    }
    .dist-row, .rank-row, .kv-row {
      display: grid;
      gap: 8px;
      align-items: center;
    }
    .dist-row { grid-template-columns: minmax(100px, 1fr) 3fr auto; }
    .rank-row { grid-template-columns: 28px 1fr auto; }
    .kv-row { grid-template-columns: minmax(130px, 0.6fr) 1fr; }
    .bar {
      height: 8px;
      border-radius: 999px;
      overflow: hidden;
      background: #e5e7eb;
    }
    .bar > span {
      display: block;
      height: 100%;
      width: var(--value);
      background: var(--brand-2);
    }
    .table-wrap {
      overflow-x: auto;
      max-height: 680px;
      border: 1px solid var(--line);
      border-radius: 8px;
      background: var(--surface);
    }
    table { width: 100%; border-collapse: collapse; min-width: 760px; }
    th, td { border-bottom: 1px solid #e7ebf0; padding: 10px 12px; text-align: left; vertical-align: top; }
    th {
      position: sticky;
      top: 0;
      z-index: 1;
      background: var(--surface-2);
      color: #344054;
      font-size: 12px;
      text-transform: uppercase;
    }
    tbody tr:nth-child(even) td { background: #fbfcfe; }
    .findings-table td:nth-child(5),
    .evidence-table td:nth-child(3) {
      max-width: 520px;
      word-break: break-word;
    }
    tr:last-child td { border-bottom: 0; }
    .badge {
      display: inline-flex;
      padding: 3px 8px;
      border-radius: 999px;
      color: #fff;
      font-size: 12px;
      font-weight: 700;
      white-space: nowrap;
    }
    .severity-info { background: var(--info); }
    .severity-low { background: var(--low); }
    .severity-medium { background: var(--medium); }
    .severity-high { background: var(--high); }
    .severity-critical { background: var(--critical); }
    .risk-meter { display: grid; grid-template-columns: 42px 96px; gap: 8px; align-items: center; }
    .risk-track { height: 7px; background: #e5e7eb; border-radius: 999px; overflow: hidden; }
    .risk-fill { height: 100%; width: var(--risk); background: var(--risk-color); }
    .component-positive { color: var(--critical); font-weight: 700; }
    .component-negative { color: var(--low); font-weight: 700; }
    .component-neutral { color: var(--muted); font-weight: 700; }
    .empty { color: var(--muted); text-align: center; padding: 20px; }
    footer {
      max-width: 1440px;
      margin: 0 auto;
      padding: 0 24px 28px;
      color: var(--muted);
      font-size: 12px;
    }
    @media (max-width: 980px) {
      .header-inner { grid-template-columns: auto 1fr; }
      .header-meta { grid-column: 1 / -1; justify-content: flex-start; }
      .grid, .columns, .two-columns, .summary-layout { grid-template-columns: 1fr; }
      .risk-ring { margin-left: 0; }
    }
    @media (max-width: 640px) {
      main, .header-inner, footer { padding-left: 14px; padding-right: 14px; }
      .header-inner { grid-template-columns: 1fr; }
      .brand-logo, .brand-fallback { width: 58px; height: 58px; }
      h1 { font-size: 22px; }
      .metric strong { font-size: 22px; }
      .panel, .metric { padding: 12px; }
      .table-wrap { max-height: 520px; }
      th, td { padding: 8px 10px; }
      .dist-row, .rank-row, .kv-row { grid-template-columns: 1fr; }
    }
    @media print {
      :root {
        --bg: #ffffff;
        --surface: #ffffff;
        --surface-2: #f3f4f6;
        --line: #cfd6df;
        --text: #111827;
        --muted: #475467;
      }
      body { background: #ffffff; font-size: 11px; }
      header {
        color: #111827;
        background: #ffffff;
        border-bottom: 2px solid #111827;
      }
      main, .header-inner, footer {
        max-width: none;
        padding: 12px;
      }
      section, .panel, .metric {
        break-inside: avoid;
        box-shadow: none;
      }
      .header-meta { justify-content: flex-start; }
      .pill {
        color: #111827;
        background: #ffffff;
        border-color: #cfd6df;
      }
      .table-wrap {
        max-height: none;
        overflow: visible;
      }
      table {
        min-width: 0;
        font-size: 10px;
      }
      th { position: static; }
      a, code { color: #111827; }
    }
  </style>
</head>
<body>
<header>
  <div class="header-inner">
    {{logoMarkup}}
    <div>
      <h1>Karakol Scan Report</h1>
      <div>Source: <code>{{Encode(report.Source.FilePath)}}</code></div>
      <div class="muted">Generated at {{Encode(generatedAt)}} by {{Encode(report.ToolName)}} {{Encode(report.ToolVersion)}}</div>
    </div>
    <div class="header-meta">
      <span class="pill">Local-first</span>
      <span class="pill">External API: {{Encode(externalApiState)}}</span>
      <span class="pill">{{Encode(report.Source.Format.ToString())}}</span>
    </div>
  </div>
</header>
<main>
  <section class="grid" data-section="metrics">
    <div class="metric risk-hero"><div class="metric-label">Overall Risk</div><strong>{{report.Summary.OverallRisk.Value}}/100</strong><small>{{Encode(report.Summary.OverallRisk.Severity.ToString())}} severity</small></div>
    <div class="metric"><div class="metric-label">Parsed Events</div><strong>{{report.Summary.ParsedEvents}}</strong><small>{{parserQuality}}% parser coverage</small></div>
    <div class="metric"><div class="metric-label">Findings</div><strong>{{report.Summary.FindingsCount}}</strong><small>{{report.Summary.SuspiciousEvents}} suspicious events</small></div>
    <div class="metric"><div class="metric-label">Failed Lines</div><strong>{{report.Summary.FailedLines}}</strong><small>{{report.Summary.TotalLines}} total lines</small></div>
  </section>
  <section class="panel summary-layout" data-section="executive-summary">
    <div>
      <h2>Executive Summary</h2>
      <p>Karakol analyzed <strong>{{report.Summary.TotalLines}}</strong> lines locally and produced <strong>{{report.Summary.FindingsCount}}</strong> findings. No external API call is required by the MVP scan path.</p>
      <p class="muted">{{Encode(report.Summary.OverallRisk.Explanation)}}</p>
    </div>
    <div class="risk-ring"><span>{{report.Summary.OverallRisk.Value}}</span></div>
  </section>
  <section class="two-columns">
    <div class="panel" data-section="severity-distribution"><h2>Severity Distribution</h2>{{severities}}</div>
    <div class="panel" data-section="threat-category-distribution"><h2>Threat Category Distribution</h2>{{categories}}</div>
  </section>
  <section class="columns">
    <div class="panel" data-section="top-source-ips"><h2>Top Source IPs</h2>{{topIps}}</div>
    <div class="panel" data-section="top-target-urls"><h2>Top Target URLs</h2>{{topUrls}}</div>
    <div class="panel" data-section="top-user-agents"><h2>Top User Agents</h2>{{topUserAgents}}</div>
  </section>
  <section class="panel" data-section="timeline">
    <h2>Timeline</h2>
    <div class="table-wrap"><table><thead><tr><th>Bucket</th><th>Events</th><th>Findings</th></tr></thead><tbody>{{Fallback(timeline, emptyTimelineRows)}}</tbody></table></div>
  </section>
  <section class="panel" data-section="critical-findings">
    <h2>Critical Findings</h2>
    <div class="table-wrap"><table><thead><tr><th>Severity</th><th>Category</th><th>Risk</th><th>Target</th><th>Recommended Action</th></tr></thead><tbody>{{Fallback(criticalRows, emptyCriticalRows)}}</tbody></table></div>
  </section>
  <section class="panel" data-section="all-findings">
    <h2>All Findings</h2>
    <div class="table-wrap"><table class="findings-table"><thead><tr><th>Severity</th><th>Category</th><th>Risk</th><th>Source IP</th><th>Target</th><th>Title</th><th>Reason</th></tr></thead><tbody>{{Fallback(rows, emptyFindingRows)}}</tbody></table></div>
  </section>
  <section class="panel" data-section="evidence-details">
    <h2>Evidence Details</h2>
    <div class="table-wrap"><table class="evidence-table"><thead><tr><th>Detector</th><th>Field</th><th>Value</th><th>Pattern</th><th>Explanation</th></tr></thead><tbody>{{Fallback(evidence, emptyEvidenceRows)}}</tbody></table></div>
  </section>
  <section class="panel" data-section="risk-components">
    <h2>Risk Components</h2>
    <div class="table-wrap"><table><thead><tr><th>Detector</th><th>Component</th><th>Contribution</th><th>Explanation</th></tr></thead><tbody>{{Fallback(riskComponents, emptyRiskRows)}}</tbody></table></div>
  </section>
  <section class="two-columns">
    <div class="panel" data-section="recommended-actions"><h2>Recommended Actions</h2>{{recommendations}}</div>
    <div class="panel" data-section="metadata"><h2>Metadata</h2>{{metadata}}</div>
  </section>
</main>
<footer>Karakol report output is encoded and generated as a local single-file artifact.</footer>
</body>
</html>
""";
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private static string ToRankedList(IEnumerable<(string Label, string Value)> values)
    {
        var items = values.ToArray();
        return items.Length == 0
            ? """<div class="empty">None</div>"""
            : "<ol class=\"rank-list\">" + string.Join(Environment.NewLine, items.Select((item, index) =>
                $"<li class=\"rank-row\"><span>{index + 1}</span><span>{Encode(item.Label)}</span><strong>{Encode(item.Value)}</strong></li>")) + "</ol>";
    }

    private static string ToActionList(IEnumerable<string> values)
    {
        var items = values.ToArray();
        return items.Length == 0
            ? """<div class="empty">No recommendations.</div>"""
            : "<ul class=\"action-list\">" + string.Join(Environment.NewLine, items.Select(item => $"<li>{Encode(item)}</li>")) + "</ul>";
    }

    private static string ToKeyValueList(IEnumerable<(string Key, string Value)> values)
    {
        var items = values.ToArray();
        return items.Length == 0
            ? """<div class="empty">No metadata.</div>"""
            : "<dl class=\"kv-list\">" + string.Join(Environment.NewLine, items.Select(item =>
                $"<div class=\"kv-row\"><dt>{Encode(item.Key)}</dt><dd>{Encode(item.Value)}</dd></div>")) + "</dl>";
    }

    private static string DistributionBars(IEnumerable<(string Label, int Count)> values)
    {
        var items = values.ToArray();
        if (items.Length == 0)
        {
            return """<div class="empty">No data.</div>""";
        }

        var max = Math.Max(1, items.Max(item => item.Count));
        return "<ul class=\"dist-list\">" + string.Join(Environment.NewLine, items.Select(item =>
        {
            var percentage = Math.Clamp(item.Count * 100 / max, 0, 100);
            return $"<li class=\"dist-row\"><span>{Encode(item.Label)}</span><span class=\"bar\" style=\"--value:{percentage}%\"><span></span></span><strong>{item.Count}</strong></li>";
        })) + "</ul>";
    }

    private static string SeverityDistribution(IReadOnlyDictionary<Severity, int> values)
    {
        var ordered = new[] { Severity.Critical, Severity.High, Severity.Medium, Severity.Low, Severity.Info }
            .Select(severity => (severity.ToString(), values.TryGetValue(severity, out var count) ? count : 0));

        return DistributionBars(ordered);
    }

    private static string SeverityBadge(Severity severity)
    {
        var css = severity.ToString().ToLowerInvariant();
        return $"<span class=\"badge severity-{css}\">{Encode(severity.ToString())}</span>";
    }

    private static string RiskMeter(int risk)
    {
        var normalized = Math.Clamp(risk, 0, 100);
        var color = normalized switch
        {
            >= 90 => "var(--critical)",
            >= 70 => "var(--high)",
            >= 40 => "var(--medium)",
            >= 10 => "var(--low)",
            _ => "var(--info)"
        };

        return $"<span class=\"risk-meter\"><strong>{normalized}</strong><span class=\"risk-track\"><span class=\"risk-fill\" style=\"--risk:{normalized}%;--risk-color:{color}\"></span></span></span>";
    }

    private static string ComponentValue(int contribution)
    {
        var css = contribution switch
        {
            > 0 => "component-positive",
            < 0 => "component-negative",
            _ => "component-neutral"
        };

        return $"<span class=\"{css}\">{contribution:+#;-#;0}</span>";
    }

    private static string SeverityColor(Severity severity)
    {
        return severity switch
        {
            Severity.Critical => "var(--critical)",
            Severity.High => "var(--high)",
            Severity.Medium => "var(--medium)",
            Severity.Low => "var(--low)",
            _ => "var(--info)"
        };
    }

    private static string Fallback(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string? TryLoadLogoDataUri()
    {
        foreach (var path in CandidateLogoPaths())
        {
            if (!File.Exists(path))
            {
                continue;
            }

            var bytes = File.ReadAllBytes(path);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }

        return null;
    }

    private static IEnumerable<string> CandidateLogoPaths()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "karakol_logo.png");
        yield return Path.Combine(Directory.GetCurrentDirectory(), "karakol_logo.png");

        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory.Parent is not null)
        {
            directory = directory.Parent;
            yield return Path.Combine(directory.FullName, "karakol_logo.png");
        }
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
