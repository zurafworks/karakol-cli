using Karakol.Domain.Enums;

namespace Karakol.Parsing.Abstractions;

public interface IParserRegistry
{
    IReadOnlyCollection<ILogParser> GetParsers();

    ILogParser? Resolve(LogFormat format);
}
