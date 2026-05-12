using Karakol.Domain.Enums;
using Karakol.Detection.Rules.Patterns;

namespace Karakol.Detection.Rules.Recon;

public sealed class ScannerBotRule() : PatternDetectionRule(ScannerBotPatterns.Basic)
{
    public override string RuleId => RuleIds.ScannerBotBasic;
    public override string Name => "Scanner Bot Activity";
    public override string Description => "Detects known scanner paths and user-agent signatures.";
    public override ThreatCategory Category => ThreatCategory.ScannerBot;
    public override Severity DefaultSeverity => Severity.Medium;
    protected override string RecommendedAction => "Rate limit the source, block confirmed scanner signatures, and review adjacent requests for exploit attempts.";

    protected override bool IsPatternMatch(Domain.Events.SecurityEvent securityEvent, string text, string pattern)
    {
        if (!pattern.StartsWith("/", StringComparison.Ordinal))
        {
            return base.IsPatternMatch(securityEvent, text, pattern);
        }

        var path = securityEvent.Path;
        return path is not null &&
            (string.Equals(path, pattern, StringComparison.OrdinalIgnoreCase) ||
             path.StartsWith($"{pattern}/", StringComparison.OrdinalIgnoreCase));
    }
}
