namespace Karakol.Domain.Scans;

public readonly record struct ScanId(Guid Value)
{
    public static ScanId New() => new(Guid.NewGuid());
}
