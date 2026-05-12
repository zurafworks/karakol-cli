using System.Text.RegularExpressions;
using Karakol.Detection.Abstractions;
using Karakol.Detection.Context;
using Karakol.Detection.Options;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Findings;

namespace Karakol.Detection.Rules.SensitiveData;

public sealed class SuspiciousUserAgentRule : IDetectionRule
{
    private static readonly Regex ScannerUaRegex = new("sqlmap|nikto|masscan|nmap|zgrab|gobuster|dirbuster|nuclei|python-requests|curl|wget", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string RuleId => RuleIds.SuspiciousUserAgentBasic;
    public string Name => "Suspicious User-Agent";
    public string Description => "Detects empty, short, or known scanner user-agent values.";
    public ThreatCategory Category => ThreatCategory.SuspiciousUserAgent;
    public Severity DefaultSeverity => Severity.Medium;
    public RuleScope Scope => RuleScope.SingleEvent;

    public bool IsEnabled(RuleOptions options) => options.Enabled && options.DisabledRules?.Contains(RuleId, StringComparer.OrdinalIgnoreCase) != true;

    public bool CanAnalyze(SecurityEvent securityEvent) => true;

    public DetectionFinding? Analyze(SecurityEvent securityEvent, RuleExecutionContext context)
    {
        if (!IsEnabled(context.Options))
        {
            return null;
        }

        var userAgent = securityEvent.UserAgent;
        var reason = string.IsNullOrWhiteSpace(userAgent)
            ? "User-Agent is empty."
            : userAgent.Length < 4
                ? "User-Agent is unusually short."
                : ScannerUaRegex.IsMatch(userAgent)
                    ? "User-Agent matches a known scanner signature."
                    : null;

        return reason is null
            ? null
            : new DetectionFinding(Category, context.Options.ResolveSeverity(RuleId, DefaultSeverity), 0.8, RuleId, Name, DetectionType.Rule, Name, Description, reason, [new Evidence("UserAgent", userAgent, null, reason)], "Review source IP reputation and consider rate limiting.")
            {
                EventId = securityEvent.Id,
                SourceIp = securityEvent.SourceIp,
                TargetResource = securityEvent.Path ?? securityEvent.Url,
                RelatedEvents = [securityEvent.Id],
                Tags = [Category.ToString()]
            };
    }
}
