using Karakol.Domain.Enums;
using Karakol.Detection.Rules.Patterns;

namespace Karakol.Detection.Rules.Web;

public sealed class XssRule() : PatternDetectionRule(XssPatterns.Basic)
{
    public override string RuleId => RuleIds.XssBasic;
    public override string Name => "Cross-Site Scripting";
    public override string Description => "Detects common XSS payloads in URL, query string, and raw messages.";
    public override ThreatCategory Category => ThreatCategory.Xss;
    public override Severity DefaultSeverity => Severity.High;
    protected override string RecommendedAction => "Verify output encoding and input validation for reflected parameters on the affected endpoint.";
}
