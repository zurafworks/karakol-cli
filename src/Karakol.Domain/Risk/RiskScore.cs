using Karakol.Domain.Enums;

namespace Karakol.Domain.Risk;

public sealed record RiskScore
{
    public RiskScore(int value, IReadOnlyCollection<RiskComponent>? components = null, string? explanation = null)
    {
        Value = Math.Clamp(value, 0, 100);
        Severity = FromValue(Value);
        Components = components ?? Array.Empty<RiskComponent>();
        Explanation = explanation ?? $"{Severity} risk ({Value}/100).";
    }

    public int Value { get; }

    public Severity Severity { get; }

    public IReadOnlyCollection<RiskComponent> Components { get; }

    public string Explanation { get; }

    public static Severity FromValue(int value) => value switch
    {
        <= 9 => Severity.Info,
        <= 39 => Severity.Low,
        <= 69 => Severity.Medium,
        <= 89 => Severity.High,
        _ => Severity.Critical
    };
}
