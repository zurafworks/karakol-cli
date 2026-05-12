using System.Runtime.CompilerServices;
using System.Text;
using Karakol.Domain.Sources;
using Karakol.Parsing.Abstractions;

namespace Karakol.Parsing.Input;

public sealed class PhysicalLogInputReader : ILogInputReader
{
    public async IAsyncEnumerable<LogLine> ReadLinesAsync(
        LogSource source,
        ReadOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var encoding = options.Encoding ?? Encoding.UTF8;
        using var stream = new FileStream(source.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, options.BufferSize, useAsync: true);
        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: options.BufferSize);
        var lineNumber = 0;

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
            {
                break;
            }

            lineNumber++;
            if (options.MaxLines is not null && lineNumber > options.MaxLines.Value)
            {
                break;
            }

            yield return new LogLine(lineNumber, line);
        }
    }
}
