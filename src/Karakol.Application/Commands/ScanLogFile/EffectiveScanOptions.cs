using Karakol.Application.Configuration;
using Karakol.Application.Policies;

namespace Karakol.Application.Commands.ScanLogFile;

internal sealed record EffectiveScanOptions(
    string OutputDirectory,
    IReadOnlyCollection<string> ReportFormats,
    bool EnableRules,
    bool EnableMl,
    bool EnableCorrelation,
    string? MinimumSeverity,
    int? MaxLines,
    bool MaskSensitiveData,
    bool IncludeRawSamples,
    PolicyContext Policy)
{
    public static EffectiveScanOptions From(ScanLogFileCommand command, KarakolSettings settings, PolicyContext policy)
    {
        var reportFormats = command.ReportFormats.Count > 0
            ? command.ReportFormats
            : settings.Reporting.DefaultFormats;

        var outputDirectory = command.OutputDirectory == "./reports"
            ? settings.Reporting.DefaultOutputDirectory
            : command.OutputDirectory;

        return new EffectiveScanOptions(
            outputDirectory,
            reportFormats,
            command.EnableRules && settings.Rules.Enabled,
            command.EnableMl || settings.ML.Enabled,
            command.EnableCorrelation,
            command.MinimumSeverity ?? policy.MinimumSeverity,
            command.MaxLines ?? settings.Scanning.MaxLines,
            command.MaskSensitiveData && settings.Scanning.MaskSensitiveData,
            command.IncludeRawSamples || settings.Scanning.IncludeRawSamples || settings.Reporting.IncludeRawSamples,
            policy);
    }
}
