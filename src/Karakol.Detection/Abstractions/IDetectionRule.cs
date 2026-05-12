using Karakol.Detection.Context;
using Karakol.Detection.Options;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Findings;

namespace Karakol.Detection.Abstractions;

public interface IDetectionRule
{
    string RuleId { get; }
    string Name { get; }
    string Description { get; }
    ThreatCategory Category { get; }
    Severity DefaultSeverity { get; }
    RuleScope Scope { get; }
    bool IsEnabled(RuleOptions options);
    bool CanAnalyze(SecurityEvent securityEvent);
    DetectionFinding? Analyze(SecurityEvent securityEvent, RuleExecutionContext context);
}
