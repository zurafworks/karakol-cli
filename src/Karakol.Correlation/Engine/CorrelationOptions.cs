namespace Karakol.Correlation.Engine;

public sealed record CorrelationOptions(
    TimeSpan? BruteForceWindow = null,
    int BruteForceThreshold = 10,
    int BruteForceCriticalThreshold = 50,
    TimeSpan? DosWindow = null,
    int DosThreshold = 500,
    int RelatedEventLimit = 20)
{
    public TimeSpan EffectiveBruteForceWindow => BruteForceWindow ?? TimeSpan.FromMinutes(5);
    public TimeSpan EffectiveDosWindow => DosWindow ?? TimeSpan.FromSeconds(30);
}
