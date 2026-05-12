using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.ML.Abstractions;
using Karakol.ML.Results;

namespace Karakol.ML.Dummy;

public sealed class DummyThreatClassifier : IThreatClassifier
{
    public bool IsAvailable => false;
    public string ModelName => "dummy";
    public string ModelVersion => "0.0.0";

    public ThreatClassificationResult Classify(SecurityEvent securityEvent)
    {
        return new ThreatClassificationResult(ThreatCategory.Normal, 0, new Dictionary<ThreatCategory, double> { [ThreatCategory.Normal] = 1 }, ModelVersion);
    }
}
