using Karakol.Domain.Enums;
using Karakol.Parsing.Abstractions;

namespace Karakol.Parsing.Registry;

public sealed class ParserRegistry(IEnumerable<ILogParser> parsers) : IParserRegistry
{
    private readonly IReadOnlyCollection<ILogParser> _parsers = parsers.OrderByDescending(parser => parser.Priority).ToArray();

    public IReadOnlyCollection<ILogParser> GetParsers() => _parsers;

    public ILogParser? Resolve(LogFormat format) => _parsers.FirstOrDefault(parser => parser.Format == format);
}
