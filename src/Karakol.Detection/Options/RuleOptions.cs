using Karakol.Domain.Enums;

namespace Karakol.Detection.Options;

public sealed record RuleOptions(
    bool Enabled = true,
    IReadOnlyCollection<string>? DisabledRules = null,
    IReadOnlyCollection<string>? EnabledRules = null,
    IReadOnlyDictionary<string, Severity>? SeverityOverrides = null)
{
    public Severity ResolveSeverity(string ruleId, Severity defaultSeverity)
    {
        return SeverityOverrides is not null && SeverityOverrides.TryGetValue(ruleId, out var severity)
            ? severity
            : defaultSeverity;
    }
}
