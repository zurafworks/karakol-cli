using Karakol.Domain.Events;
using Karakol.ML.Features;

namespace Karakol.ML.Abstractions;

public interface IEventFeatureExtractor
{
    ThreatModelInput Extract(SecurityEvent securityEvent);
}
