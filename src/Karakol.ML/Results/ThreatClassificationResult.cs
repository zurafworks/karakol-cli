using Karakol.Domain.Enums;

namespace Karakol.ML.Results;

public sealed record ThreatClassificationResult(
    ThreatCategory PredictedCategory,
    double Confidence,
    IReadOnlyDictionary<ThreatCategory, double> Scores,
    string ModelVersion);
