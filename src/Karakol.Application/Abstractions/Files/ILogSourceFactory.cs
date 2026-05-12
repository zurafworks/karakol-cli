using Karakol.Domain.Enums;
using Karakol.Domain.Sources;

namespace Karakol.Application.Abstractions.Files;

public interface ILogSourceFactory
{
    LogSource Create(string filePath, LogFormat format);
}
