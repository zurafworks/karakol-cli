namespace Karakol.Application.Configuration;

public sealed class ReportingOptions
{
    public string DefaultOutputDirectory { get; init; } = "./reports";
    public IReadOnlyCollection<string> DefaultFormats { get; init; } = ["json"];
    public bool IncludeEvidence { get; init; } = true;
    public bool IncludeRawSamples { get; init; }
}
