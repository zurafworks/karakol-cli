namespace Karakol.Application.Policies;

public sealed class PolicyContext
{
    public string? MinimumSeverity { get; init; }
    public IReadOnlyCollection<string> EnabledRules { get; init; } = [];
    public IReadOnlyCollection<string> DisabledRules { get; init; } = [];
    public IReadOnlyCollection<string> SensitivePaths { get; init; } = [];
    public IReadOnlyCollection<string> TrustedIps { get; init; } = [];
    public IReadOnlyCollection<string> IgnoredPaths { get; init; } = [];

    public static PolicyContext Default { get; } = new();
}
