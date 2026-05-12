using Karakol.Domain.Enums;

namespace Karakol.Domain.Sources;

public sealed record LogSource(
    LogSourceId Id,
    string FilePath,
    LogFormat Format,
    long SizeInBytes,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? LastModifiedAt,
    LogSourceType SourceType);
