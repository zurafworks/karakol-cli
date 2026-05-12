using Karakol.Domain.Enums;
using Karakol.Detection.Rules.Patterns;

namespace Karakol.Detection.Rules.Injection;

public sealed class CommandInjectionRule() : PatternDetectionRule(CommandInjectionPatterns.Basic)
{
    public override string RuleId => RuleIds.CommandInjectionBasic;
    public override string Name => "Command Injection";
    public override string Description => "Detects shell metacharacters and command execution payloads.";
    public override ThreatCategory Category => ThreatCategory.CommandInjection;
    public override Severity DefaultSeverity => Severity.High;
    protected override string RecommendedAction => "Avoid shell execution with user input, validate command parameters, and review host command execution logs.";
}
