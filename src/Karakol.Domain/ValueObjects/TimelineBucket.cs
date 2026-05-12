namespace Karakol.Domain.ValueObjects;

public sealed record TimelineBucket(DateTimeOffset? StartsAt, int EventCount, int FindingCount);
