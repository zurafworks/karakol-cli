using Karakol.Domain.Sources;
using Karakol.Parsing.Input;

namespace Karakol.Parsing.Abstractions;

public interface ILogInputReader
{
    IAsyncEnumerable<LogLine> ReadLinesAsync(LogSource source, ReadOptions options, CancellationToken cancellationToken);
}
