using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Findings;
using Karakol.Correlation.Engine;

namespace Karakol.Correlation.Rules;

public static class BruteForceCorrelationRule
{
    public static IEnumerable<DetectionFinding> Detect(IReadOnlyCollection<SecurityEvent> events, CorrelationOptions options)
    {
        var candidates = events
            .Where(securityEvent =>
                !string.IsNullOrWhiteSpace(securityEvent.SourceIp) &&
                (securityEvent.EventType == "failed_login" ||
                 securityEvent.StatusCode is 401 or 403 ||
                 securityEvent.Path?.Contains("login", StringComparison.OrdinalIgnoreCase) == true))
            .GroupBy(securityEvent => securityEvent.SourceIp!);

        foreach (var group in candidates)
        {
            var window = SelectMaxWindow(group, options.EffectiveBruteForceWindow, options.RelatedEventLimit);
            var count = window.Count;
            if (count < options.BruteForceThreshold)
            {
                continue;
            }

            var severity = count >= options.BruteForceCriticalThreshold ? Severity.Critical : Severity.Medium;
            yield return new DetectionFinding(
                ThreatCategory.BruteForce,
                severity,
                0.88,
                "bruteforce.auth",
                "Brute Force Attempt",
                DetectionType.Correlation,
                "Brute Force Attempt",
                "Detects repeated authentication failures or login attempts from the same source IP.",
                $"{group.Key} generated {count} failed or suspicious login events.",
                [new Evidence("SourceIp", group.Key, count.ToString(), $"Repeated failed or suspicious login activity inside {options.EffectiveBruteForceWindow}.")],
                "Temporarily block the source IP and review authentication controls.")
            {
                SourceIp = group.Key,
                RelatedEvents = window.Take(options.RelatedEventLimit).Select(securityEvent => securityEvent.Id).ToArray(),
                Tags = ["correlation", "bruteforce"]
            };
        }
    }

    private static IReadOnlyCollection<SecurityEvent> SelectMaxWindow(IEnumerable<SecurityEvent> events, TimeSpan windowSize, int relatedEventLimit)
    {
        var ordered = events.OrderBy(securityEvent => securityEvent.Timestamp ?? DateTimeOffset.MinValue).ToArray();
        if (ordered.All(securityEvent => securityEvent.Timestamp is null))
        {
            return ordered;
        }

        var best = Array.Empty<SecurityEvent>();
        var window = new Queue<SecurityEvent>();
        foreach (var securityEvent in ordered)
        {
            if (securityEvent.Timestamp is null)
            {
                continue;
            }

            window.Enqueue(securityEvent);
            while (window.TryPeek(out var first) &&
                   first.Timestamp is not null &&
                   securityEvent.Timestamp.Value - first.Timestamp.Value > windowSize)
            {
                window.Dequeue();
            }

            if (window.Count > best.Length)
            {
                best = window.ToArray();
            }
        }

        return best;
    }
}
