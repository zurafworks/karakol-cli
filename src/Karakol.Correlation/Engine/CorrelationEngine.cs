using Karakol.Domain.Events;
using Karakol.Domain.Findings;
using Karakol.Correlation.Rules;

namespace Karakol.Correlation.Engine;

public sealed class CorrelationEngine
{
    public IReadOnlyCollection<DetectionFinding> Analyze(IReadOnlyCollection<SecurityEvent> events, CorrelationOptions? options = null)
    {
        options ??= new CorrelationOptions();
        var findings = new List<DetectionFinding>();
        findings.AddRange(BruteForceCorrelationRule.Detect(events, options));
        findings.AddRange(DosAttemptCorrelationRule.Detect(events, options));
        return findings;
    }
}
