using Karakol.Parsing.Abstractions;
using Karakol.Plugins.Abstractions.Core;

namespace Karakol.Plugins.Abstractions.Parsers;

public interface IParserPlugin : IPlugin
{
    IReadOnlyCollection<ILogParser> GetParsers();
}
