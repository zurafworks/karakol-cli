namespace Karakol.ML.Features;

public sealed record ThreatModelInput(string NormalizedText, IReadOnlyDictionary<string, float> NumericFeatures);
