namespace Karakol.Domain.Events;

public readonly record struct SecurityEventId(Guid Value)
{
    public static SecurityEventId New() => new(Guid.NewGuid());
}
