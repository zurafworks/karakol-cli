using Karakol.Domain.Enums;
using Karakol.Detection.Rules.Patterns;

namespace Karakol.Detection.Rules.SensitiveData;

public sealed class SensitiveFileAccessRule() : PatternDetectionRule(SensitiveFilePatterns.Basic)
{
    public override string RuleId => RuleIds.SensitiveFileBasic;
    public override string Name => "Sensitive File Access";
    public override string Description => "Detects attempts to access common sensitive files.";
    public override ThreatCategory Category => ThreatCategory.SensitiveFileAccess;
    public override Severity DefaultSeverity => Severity.High;
    protected override string RecommendedAction => "Ensure sensitive files are outside public roots and deny access to configuration, backup, and credential files.";
}
