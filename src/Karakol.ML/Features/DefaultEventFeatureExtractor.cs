using Karakol.Domain.Events;
using Karakol.ML.Abstractions;

namespace Karakol.ML.Features;

public sealed class DefaultEventFeatureExtractor : IEventFeatureExtractor
{
    public ThreatModelInput Extract(SecurityEvent securityEvent)
    {
        var text = string.Join(' ', new[]
        {
            securityEvent.HttpMethod,
            securityEvent.Path,
            securityEvent.QueryString,
            securityEvent.UserAgent,
            securityEvent.NormalizedMessage,
            securityEvent.RawMessage
        }.Where(value => !string.IsNullOrWhiteSpace(value))).ToLowerInvariant();

        return new ThreatModelInput(text, new Dictionary<string, float>
        {
            ["statusCode"] = securityEvent.StatusCode ?? 0,
            ["responseSize"] = securityEvent.ResponseSize ?? 0,
            ["hasQuery"] = string.IsNullOrWhiteSpace(securityEvent.QueryString) ? 0 : 1,
            ["rawLength"] = securityEvent.RawMessage.Length
        });
    }
}
