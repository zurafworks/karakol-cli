using Karakol.Detection.Abstractions;
using Karakol.Detection.Context;
using Karakol.Detection.Options;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Findings;

namespace Karakol.Detection.Rules;

public abstract class PatternDetectionRule : IDetectionRule
{
    private readonly IReadOnlyCollection<string> _patterns;

    protected PatternDetectionRule(IReadOnlyCollection<string> patterns)
    {
        _patterns = patterns;
    }

    public abstract string RuleId { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract ThreatCategory Category { get; }
    public abstract Severity DefaultSeverity { get; }
    public RuleScope Scope => RuleScope.SingleEvent;
    protected virtual string RecommendedAction => "Review the request source, block abusive clients if confirmed, and harden the affected endpoint.";

    public bool IsEnabled(RuleOptions options)
    {
        if (!options.Enabled || options.DisabledRules?.Contains(RuleId, StringComparer.OrdinalIgnoreCase) == true)
        {
            return false;
        }

        return options.EnabledRules is null || options.EnabledRules.Count == 0 || options.EnabledRules.Contains(RuleId, StringComparer.OrdinalIgnoreCase);
    }

    public bool CanAnalyze(SecurityEvent securityEvent) => !string.IsNullOrWhiteSpace(GetSearchText(securityEvent));

    public DetectionFinding? Analyze(SecurityEvent securityEvent, RuleExecutionContext context)
    {
        if (!IsEnabled(context.Options) || !CanAnalyze(securityEvent))
        {
            return null;
        }

        var text = GetSearchText(securityEvent);
        var matched = _patterns.FirstOrDefault(pattern => IsPatternMatch(securityEvent, text, pattern));
        if (matched is null)
        {
            return null;
        }

        var field = SelectEvidenceField(securityEvent, matched);
        var value = field switch
        {
            "UserAgent" => securityEvent.UserAgent,
            "Path" => securityEvent.Path,
            "QueryString" => securityEvent.QueryString,
            "Url" => securityEvent.Url,
            _ => securityEvent.RawMessage
        };

        return new DetectionFinding(
            Category,
            context.Options.ResolveSeverity(RuleId, DefaultSeverity),
            0.92,
            RuleId,
            Name,
            DetectionType.Rule,
            Name,
            Description,
            $"{field} contains suspicious pattern '{matched}'.",
            [new Evidence(field, value, matched, $"{field} contains pattern '{matched}'.")],
            RecommendedAction)
        {
            EventId = securityEvent.Id,
            SourceIp = securityEvent.SourceIp,
            TargetResource = securityEvent.Path ?? securityEvent.Url,
            RelatedEvents = [securityEvent.Id],
            Tags = [Category.ToString()]
        };
    }

    protected virtual string GetSearchText(SecurityEvent securityEvent)
    {
        return string.Join(' ', new[]
        {
            securityEvent.Url,
            securityEvent.Path,
            securityEvent.QueryString,
            securityEvent.UserAgent,
            securityEvent.NormalizedMessage,
            securityEvent.RawMessage
        }.Where(value => !string.IsNullOrWhiteSpace(value))).ToLowerInvariant();
    }

    protected virtual bool IsPatternMatch(SecurityEvent securityEvent, string text, string pattern)
    {
        return text.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static string SelectEvidenceField(SecurityEvent securityEvent, string matched)
    {
        if (securityEvent.UserAgent?.Contains(matched, StringComparison.OrdinalIgnoreCase) == true)
        {
            return "UserAgent";
        }

        if (securityEvent.QueryString?.Contains(matched, StringComparison.OrdinalIgnoreCase) == true)
        {
            return "QueryString";
        }

        if (securityEvent.Path?.Contains(matched, StringComparison.OrdinalIgnoreCase) == true)
        {
            return "Path";
        }

        if (securityEvent.Url?.Contains(matched, StringComparison.OrdinalIgnoreCase) == true)
        {
            return "Url";
        }

        return "RawMessage";
    }
}
