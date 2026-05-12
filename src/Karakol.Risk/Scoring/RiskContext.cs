namespace Karakol.Risk.Scoring;

public sealed class RiskContext
{
    public int RepetitionCount { get; init; }
    public int CategoryDiversity { get; init; }
    public bool IsSensitiveEndpoint { get; init; }
    public bool IsTrustedIp { get; init; }
}
