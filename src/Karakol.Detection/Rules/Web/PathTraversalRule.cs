using Karakol.Domain.Enums;
using Karakol.Detection.Rules.Patterns;

namespace Karakol.Detection.Rules.Web;

public sealed class PathTraversalRule() : PatternDetectionRule(PathTraversalPatterns.Basic)
{
    public override string RuleId => RuleIds.PathTraversalBasic;
    public override string Name => "Path Traversal";
    public override string Description => "Detects attempts to access files outside the intended web root.";
    public override ThreatCategory Category => ThreatCategory.PathTraversal;
    public override Severity DefaultSeverity => Severity.High;
    protected override string RecommendedAction => "Normalize and constrain file paths server-side, and block traversal payloads before file access.";
}
