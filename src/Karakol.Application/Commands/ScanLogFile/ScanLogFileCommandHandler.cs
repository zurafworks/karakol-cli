using Karakol.Application.Abstractions.Files;
using Karakol.Application.Configuration;
using Karakol.Application.Policies;
using Karakol.Contracts.Reports;
using Karakol.Contracts.Scan;
using Karakol.Correlation.Engine;
using Karakol.Detection.Engine;
using Karakol.Detection.Options;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Findings;
using Karakol.Domain.Risk;
using Karakol.Domain.Scans;
using Karakol.Domain.ValueObjects;
using Karakol.ML.Abstractions;
using Karakol.Parsing.Abstractions;
using Karakol.Parsing.Detection;
using Karakol.Parsing.Input;
using Karakol.Parsing.Options;
using Karakol.Persistence.Abstractions;
using Karakol.Reporting.Options;
using Karakol.Reporting.Registry;
using Karakol.Reporting.Results;
using Karakol.Application.Scanning;
using Karakol.Risk.Abstractions;
using Karakol.Risk.Scoring;
using Karakol.Shared.Errors;
using Karakol.Shared.Results;
using MediatR;

namespace Karakol.Application.Commands.ScanLogFile;

public sealed class ScanLogFileCommandHandler(
    ILogInputReader inputReader,
    IParserRegistry parserRegistry,
    ILogFormatDetector formatDetector,
    ILogSourceFactory logSourceFactory,
    SingleEventRuleEngine ruleEngine,
    CorrelationEngine correlationEngine,
    IRiskScoringService riskScoringService,
    ReportWriterRegistry reportWriterRegistry,
    IThreatClassifier threatClassifier,
    IScanConfigurationProvider configurationProvider,
    IPolicyProvider policyProvider,
    IEnumerable<IScanHistoryRepositoryProvider> scanHistoryRepositoryProviders) : IRequestHandler<ScanLogFileCommand, Result<ScanLogFileResult>>
{
    public Task<Result<ScanLogFileResult>> HandleAsync(ScanLogFileCommand command, CancellationToken cancellationToken)
    {
        return Handle(command, cancellationToken);
    }

    public async Task<Result<ScanLogFileResult>> Handle(ScanLogFileCommand command, CancellationToken cancellationToken)
    {
        var settingsResult = await configurationProvider.LoadAsync(command.ConfigurationFile, cancellationToken).ConfigureAwait(false);
        if (settingsResult.IsFailure || settingsResult.Value is null)
        {
            return Result<ScanLogFileResult>.Failure(settingsResult.Error!);
        }

        var policyResult = await policyProvider.LoadAsync(command.PolicyFile, cancellationToken).ConfigureAwait(false);
        if (policyResult.IsFailure || policyResult.Value is null)
        {
            return Result<ScanLogFileResult>.Failure(policyResult.Error!);
        }

        var options = EffectiveScanOptions.From(command, settingsResult.Value, policyResult.Value);
        var requestedFormat = ScanLogFileFormatParser.Parse(command.Format);
        var source = logSourceFactory.Create(command.FilePath, requestedFormat);
        var detectedFormat = requestedFormat;
        FormatDetectionResult? formatDetection = null;
        if (requestedFormat is LogFormat.Auto or LogFormat.Unknown)
        {
            formatDetection = await formatDetector.DetectAsync(source, cancellationToken).ConfigureAwait(false);
            detectedFormat = formatDetection.Format;
        }

        source = source with { Format = detectedFormat };
        var parser = parserRegistry.Resolve(detectedFormat) ?? parserRegistry.Resolve(LogFormat.Generic);
        if (parser is null)
        {
            return Result<ScanLogFileResult>.Failure(new Error("Parser.NotFound", $"No parser found for format '{detectedFormat}'.", Type: ErrorType.Parsing));
        }

        var lines = inputReader.ReadLinesAsync(source, new ReadOptions(options.MaxLines), cancellationToken);
        var parseResults = parser.ParseAsync(lines, new ParserOptions(source), cancellationToken);
        var events = new List<SecurityEvent>();
        var correlationEvents = new List<SecurityEvent>();
        var findings = new List<DetectionFinding>();
        var failedLines = 0;
        var totalLines = 0;
        var ruleOptions = new RuleOptions(options.EnableRules, options.Policy.DisabledRules, options.Policy.EnabledRules);

        await foreach (var parseResult in parseResults.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            totalLines = Math.Max(totalLines, parseResult.LineNumber);
            if (!parseResult.IsSuccess || parseResult.Event is null)
            {
                failedLines++;
                continue;
            }

            var securityEvent = ScanEventPreprocessor.Preprocess(parseResult.Event);
            events.Add(securityEvent);
            if (PolicyMatcher.IsIgnored(securityEvent, options.Policy))
            {
                continue;
            }

            correlationEvents.Add(securityEvent);
            if (options.EnableRules)
            {
                findings.AddRange(ruleEngine.Analyze(securityEvent, ruleOptions));
            }

            if (options.EnableMl && threatClassifier.IsAvailable)
            {
                _ = threatClassifier.Classify(securityEvent);
            }
        }

        if (options.EnableCorrelation)
        {
            findings.AddRange(correlationEngine.Analyze(correlationEvents).Where(finding => IsDetectorAllowed(finding.DetectorId, options.Policy)));
        }

        var scoredFindings = ScoreFindings(findings, options);
        var summaryWithoutRisk = ScanSummaryFactory.Build(totalLines, events, failedLines, scoredFindings, new RiskScore(0));
        var overallRisk = riskScoringService.CalculateScanRisk(summaryWithoutRisk, scoredFindings);
        var summary = ScanSummaryFactory.Build(totalLines, events, failedLines, scoredFindings, overallRisk);
        var parserFailureRatio = totalLines == 0 ? 0 : (double)failedLines / totalLines;
        var report = CreateReport(command, options, requestedFormat, source, summary, scoredFindings, formatDetection, parserFailureRatio);
        var reportFiles = await WriteReportsAsync(options, report, cancellationToken).ConfigureAwait(false);
        if (reportFiles.Error is not null)
        {
            return Result<ScanLogFileResult>.Failure(reportFiles.Error);
        }

        var persistenceResult = await PersistHistoryAsync(command, settingsResult.Value.Persistence, report, cancellationToken).ConfigureAwait(false);
        if (persistenceResult is not null)
        {
            return Result<ScanLogFileResult>.Failure(persistenceResult);
        }

        var dto = new ScanReportDto(
            report.ScanId.Value,
            source.FilePath,
            detectedFormat.ToString(),
            summary.TotalLines,
            summary.ParsedEvents,
            summary.FailedLines,
            summary.FindingsCount,
            summary.OverallRisk.Value,
            summary.OverallRisk.Severity.ToString(),
            reportFiles.Value!.Select(file => new ReportFileDto(file.Format, file.FilePath, file.SizeInBytes)).ToArray());

        return Result<ScanLogFileResult>.Success(new ScanLogFileResult(report, reportFiles.Value!, dto));
    }

    private async Task<Error?> PersistHistoryAsync(
        ScanLogFileCommand command,
        PersistenceOptions persistenceOptions,
        ScanReport report,
        CancellationToken cancellationToken)
    {
        var enabled = command.PersistHistory || persistenceOptions.Enabled;
        if (!enabled)
        {
            return null;
        }

        var providerName = persistenceOptions.Enabled ? persistenceOptions.Provider : "DuckDB";
        var options = new ScanHistoryPersistenceOptions(
            Enabled: true,
            Provider: providerName,
            ConnectionString: persistenceOptions.ConnectionString ?? "karakol.duckdb");
        var provider = scanHistoryRepositoryProviders.FirstOrDefault(candidate => candidate.CanCreate(options));
        if (provider is null)
        {
            return new Error(
                "Persistence.ProviderUnsupported",
                $"Persistence provider '{providerName}' is not available.",
                Type: ErrorType.Configuration);
        }

        await provider.Create(options).SaveAsync(report, cancellationToken).ConfigureAwait(false);
        return null;
    }

    private IReadOnlyCollection<DetectionFinding> ScoreFindings(IReadOnlyCollection<DetectionFinding> findings, EffectiveScanOptions options)
    {
        var repetitionBySource = findings
            .Where(finding => !string.IsNullOrWhiteSpace(finding.SourceIp))
            .GroupBy(finding => finding.SourceIp!)
            .ToDictionary(group => group.Key, group => group.Count());
        var categoryDiversityBySource = findings
            .Where(finding => !string.IsNullOrWhiteSpace(finding.SourceIp))
            .GroupBy(finding => finding.SourceIp!)
            .ToDictionary(group => group.Key, group => group.Select(finding => finding.Category).Distinct().Count());

        return findings
            .Select(finding =>
            {
                var repetition = finding.SourceIp is not null && repetitionBySource.TryGetValue(finding.SourceIp, out var count) ? count : 0;
                var categoryDiversity = finding.SourceIp is not null && categoryDiversityBySource.TryGetValue(finding.SourceIp, out var diversity) ? diversity : 0;
                var risk = riskScoringService.CalculateFindingRisk(finding, new RiskContext
                {
                    RepetitionCount = repetition,
                    CategoryDiversity = categoryDiversity,
                    IsSensitiveEndpoint = finding.Category == ThreatCategory.SensitiveFileAccess || PolicyMatcher.IsSensitiveEndpoint(finding.TargetResource, options.Policy),
                    IsTrustedIp = PolicyMatcher.IsTrustedIp(finding.SourceIp, options.Policy)
                });
                return finding with { RiskScore = risk };
            })
            .Where(finding => ScanSeverityFilter.Passes(finding.Severity, options.MinimumSeverity))
            .OrderByDescending(finding => finding.RiskScore.Value)
            .ToArray();
    }

    private static ScanReport CreateReport(
        ScanLogFileCommand command,
        EffectiveScanOptions options,
        LogFormat requestedFormat,
        Domain.Sources.LogSource source,
        ScanSummary summary,
        IReadOnlyCollection<DetectionFinding> findings,
        FormatDetectionResult? formatDetection,
        double parserFailureRatio)
    {
        return new ScanReport(
            ScanId.New(),
            "Karakol",
            "0.1.0",
            DateTimeOffset.UtcNow,
            source,
            summary,
            findings,
            summary.Timeline,
            summary.Recommendations,
            new ScanConfigurationSnapshot(requestedFormat, options.ReportFormats, options.EnableRules, options.EnableMl, options.EnableCorrelation, options.MaskSensitiveData, options.MaxLines),
            new Dictionary<string, string>
            {
                ["localFirst"] = "true",
                ["externalApiCalls"] = "false",
                ["configFile"] = command.ConfigurationFile ?? "",
                ["policyFile"] = command.PolicyFile ?? "",
                ["formatDetectionConfidence"] = formatDetection?.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) ?? "manual",
                ["formatDetectionEvidence"] = formatDetection is null ? "manual format selection" : string.Join(" | ", formatDetection.Evidence),
                ["parserFailureRatio"] = parserFailureRatio.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private async Task<(IReadOnlyCollection<ReportWriteResult>? Value, Error? Error)> WriteReportsAsync(EffectiveScanOptions options, ScanReport report, CancellationToken cancellationToken)
    {
        var reportFiles = new List<ReportWriteResult>();
        foreach (var format in ScanReportFormatNormalizer.Normalize(options.ReportFormats))
        {
            var writer = reportWriterRegistry.Resolve(format);
            if (writer is null)
            {
                return (null, new Error("Report.FormatUnsupported", $"Report format '{format}' is not supported.", Type: ErrorType.Reporting));
            }

            reportFiles.Add(await writer.WriteAsync(report, new ReportOptions(NormalizeOutputDirectory(options.OutputDirectory), options.MaskSensitiveData, options.IncludeRawSamples), cancellationToken).ConfigureAwait(false));
        }

        return (reportFiles, null);
    }

    private static bool IsDetectorAllowed(string detectorId, PolicyContext policy)
    {
        if (policy.DisabledRules.Contains(detectorId, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        return policy.EnabledRules.Count == 0 || policy.EnabledRules.Contains(detectorId, StringComparer.OrdinalIgnoreCase);
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
