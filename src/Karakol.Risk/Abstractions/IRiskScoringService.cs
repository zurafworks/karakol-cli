using Karakol.Domain.Findings;
using Karakol.Domain.Risk;
using Karakol.Domain.Scans;
using Karakol.Risk.Scoring;

namespace Karakol.Risk.Abstractions;

public interface IRiskScoringService
{
    RiskScore CalculateFindingRisk(DetectionFinding finding, RiskContext context);

    RiskScore CalculateScanRisk(ScanSummary summary, IReadOnlyCollection<DetectionFinding> findings);
}
