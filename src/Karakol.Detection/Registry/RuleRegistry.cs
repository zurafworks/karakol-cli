using Karakol.Detection.Abstractions;

namespace Karakol.Detection.Registry;

public sealed class RuleRegistry(IEnumerable<IDetectionRule> rules) : IRuleRegistry
{
    private readonly IReadOnlyCollection<IDetectionRule> _rules = rules.ToArray();

    public IReadOnlyCollection<IDetectionRule> GetRules() => _rules;
}
