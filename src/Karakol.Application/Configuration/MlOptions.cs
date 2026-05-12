namespace Karakol.Application.Configuration;

public sealed class MlOptions
{
    public bool Enabled { get; init; }
    public string? ModelPath { get; init; }
    public double MinimumConfidence { get; init; } = 0.70;
}
