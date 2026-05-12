using Karakol.Domain.Enums;

namespace Karakol.Domain.Scans;

public sealed record ScanConfigurationSnapshot(
    LogFormat RequestedFormat,
    IReadOnlyCollection<string> ReportFormats,
    bool RulesEnabled,
    bool MlEnabled,
    bool CorrelationEnabled,
    bool MaskSensitiveData,
    int? MaxLines);
