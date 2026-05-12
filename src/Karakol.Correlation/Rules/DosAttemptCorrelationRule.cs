using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Findings;
using Karakol.Correlation.Engine;

namespace Karakol.Correlation.Rules;

public static class DosAttemptCorrelationRule
{
    public static IEnumerable<DetectionFinding> Detect(IReadOnlyCollection<SecurityEvent> events, CorrelationOptions options)
    {
        foreach (var group in events.Where(securityEvent => !string.IsNullOrWhiteSpace(securityEvent.SourceIp)).GroupBy(securityEvent => securityEvent.SourceIp!))
        {
            var window = SelectMaxWindow(group, options.EffectiveDosWindow, options.RelatedEventLimit);
            var count = window.Count;
            if (count < options.DosThreshold)
            {
                continue;
            }

            yield return new DetectionFinding(
                ThreatCategory.DosAttempt,
                Severity.High,
                0.75,
                "dos.basic",
                "Basic DoS Attempt",
                DetectionType.Correlation,
                "High Request Volume",
                "Detects high request volume from a single source IP.",
                $"{group.Key} generated {count} requests in the analyzed sample.",
                [new Evidence("SourceIp", group.Key, count.ToString(), $"High request volume inside {options.EffectiveDosWindow}.")],
                "Review traffic rate and apply rate limiting or upstream filtering.")
            {
                SourceIp = group.Key,
                RelatedEvents = window.Take(options.RelatedEventLimit).Select(securityEvent => securityEvent.Id).ToArray(),
                Tags = ["correlation", "dos"]
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
