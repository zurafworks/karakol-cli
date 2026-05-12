using Karakol.Domain.Enums;
using Karakol.Detection.Rules.Patterns;

namespace Karakol.Detection.Rules.Injection;

public sealed class SqlInjectionRule() : PatternDetectionRule(SqlInjectionPatterns.Basic)
{
    public override string RuleId => RuleIds.SqlInjectionBasic;
    public override string Name => "SQL Injection";
    public override string Description => "Detects common SQL injection payloads in request data.";
    public override ThreatCategory Category => ThreatCategory.SqlInjection;
    public override Severity DefaultSeverity => Severity.High;
    protected override string RecommendedAction => "Use parameterized queries, validate user input, and review database-backed endpoint logs around this request.";
}
