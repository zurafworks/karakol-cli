namespace Karakol.Domain.Findings;

public readonly record struct DetectionFindingId(Guid Value)
{
    public static DetectionFindingId New() => new(Guid.NewGuid());
}
