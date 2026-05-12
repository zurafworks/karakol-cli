namespace Karakol.Application.Configuration;

public sealed class ScanningOptions
{
    public int? MaxLines { get; init; }
    public int StreamingBatchSize { get; init; } = 1000;
    public bool AutoDetectFormat { get; init; } = true;
    public bool MaskSensitiveData { get; init; } = true;
    public bool IncludeRawSamples { get; init; }
}
