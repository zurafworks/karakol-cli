namespace Karakol.Application.Configuration;

public sealed class KarakolSettings
{
    public ScanningOptions Scanning { get; init; } = new();
    public ApplicationRuleOptions Rules { get; init; } = new();
    public MlOptions ML { get; init; } = new();
    public ReportingOptions Reporting { get; init; } = new();
    public PersistenceOptions Persistence { get; init; } = new();

    public static KarakolSettings Defaults { get; } = new();
}
