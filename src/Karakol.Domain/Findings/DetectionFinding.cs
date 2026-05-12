using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Risk;
using Karakol.Shared.Guards;

namespace Karakol.Domain.Findings;

public sealed record DetectionFinding
{
    public DetectionFinding(
        ThreatCategory category,
        Severity severity,
        double confidence,
        string detectorId,
        string detectorName,
        DetectionType detectionType,
        string title,
        string description,
        string reason,
        IReadOnlyCollection<Evidence> evidence,
        string recommendedAction)
    {
        Id = DetectionFindingId.New();
        Category = category;
        Severity = severity;
        Confidence = Math.Clamp(confidence, 0, 1);
        DetectorId = Guard.NotNullOrWhiteSpace(detectorId, nameof(detectorId));
        DetectorName = Guard.NotNullOrWhiteSpace(detectorName, nameof(detectorName));
        DetectionType = detectionType;
        Title = Guard.NotNullOrWhiteSpace(title, nameof(title));
        Description = Guard.NotNullOrWhiteSpace(description, nameof(description));
        Reason = Guard.NotNullOrWhiteSpace(reason, nameof(reason));
        Evidence = evidence;
        RecommendedAction = recommendedAction;
        CreatedAt = DateTimeOffset.UtcNow;
        RiskScore = new RiskScore(0);
    }

    public DetectionFindingId Id { get; init; }
    public SecurityEventId? EventId { get; init; }
    public ThreatCategory Category { get; init; }
    public Severity Severity { get; init; }
    public RiskScore RiskScore { get; init; }
    public double Confidence { get; init; }
    public string DetectorId { get; init; }
    public string DetectorName { get; init; }
    public DetectionType DetectionType { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public string Reason { get; init; }
    public IReadOnlyCollection<Evidence> Evidence { get; init; }
    public string RecommendedAction { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyCollection<SecurityEventId> RelatedEvents { get; init; } = Array.Empty<SecurityEventId>();
    public string? SourceIp { get; init; }
    public string? TargetResource { get; init; }
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();
}
