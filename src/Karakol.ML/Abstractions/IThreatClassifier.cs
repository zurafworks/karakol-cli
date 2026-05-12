using Karakol.Domain.Events;
using Karakol.ML.Results;

namespace Karakol.ML.Abstractions;

public interface IThreatClassifier
{
    bool IsAvailable { get; }
    string ModelName { get; }
    string ModelVersion { get; }
    ThreatClassificationResult Classify(SecurityEvent securityEvent);
}
