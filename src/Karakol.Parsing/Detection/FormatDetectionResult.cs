using Karakol.Domain.Enums;

namespace Karakol.Parsing.Detection;

public sealed record FormatDetectionResult(LogFormat Format, double Confidence, IReadOnlyCollection<string> Evidence);
