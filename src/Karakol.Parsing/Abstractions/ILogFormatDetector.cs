using Karakol.Domain.Sources;
using Karakol.Parsing.Detection;

namespace Karakol.Parsing.Abstractions;

public interface ILogFormatDetector
{
    Task<FormatDetectionResult> DetectAsync(LogSource source, CancellationToken cancellationToken);
}
