using Karakol.Detection.Abstractions;
using Karakol.Detection.Context;
using Karakol.Detection.Options;
using Karakol.Domain.Events;
using Karakol.Domain.Findings;

namespace Karakol.Detection.Engine;

public sealed class SingleEventRuleEngine(IRuleRegistry ruleRegistry)
{
    public IReadOnlyCollection<DetectionFinding> Analyze(SecurityEvent securityEvent, RuleOptions options)
    {
        var context = new RuleExecutionContext(options);
        return ruleRegistry.GetRules()
            .Where(rule => rule.Scope == RuleScope.SingleEvent)
            .Select(rule => rule.Analyze(securityEvent, context))
            .Where(finding => finding is not null)
            .Select(finding => finding!)
            .ToArray();
    }
}
