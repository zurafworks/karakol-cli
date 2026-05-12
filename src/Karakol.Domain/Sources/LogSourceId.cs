namespace Karakol.Domain.Sources;

public readonly record struct LogSourceId(Guid Value)
{
    public static LogSourceId New() => new(Guid.NewGuid());
}
