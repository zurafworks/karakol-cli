using Karakol.Application.Abstractions.Files;
using Karakol.Domain.Enums;
using Karakol.Domain.Sources;

namespace Karakol.Infrastructure.Files;

public sealed class FileMetadataProvider : ILogSourceFactory
{
    public LogSource Create(string filePath, LogFormat format)
    {
        var fileInfo = new FileInfo(filePath);
        return new LogSource(
            LogSourceId.New(),
            fileInfo.FullName,
            format,
            fileInfo.Length,
            fileInfo.CreationTimeUtc,
            fileInfo.LastWriteTimeUtc,
            LogSourceType.File);
    }
}
