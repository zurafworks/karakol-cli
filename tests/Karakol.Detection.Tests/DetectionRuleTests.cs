using FluentAssertions;
using Karakol.Detection.Abstractions;
using Karakol.Detection.Context;
using Karakol.Detection.Options;
using Karakol.Detection.Rules.Injection;
using Karakol.Detection.Rules.Recon;
using Karakol.Detection.Rules.SensitiveData;
using Karakol.Detection.Rules.Web;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Sources;

namespace Karakol.Detection.Tests;

public sealed class DetectionRuleTests
{
    [Fact]
    public void SqlInjectionRule_detects_union_select()
    {
        Analyze(new SqlInjectionRule(), Event(url: "/login?id=1 union select password from users")).Should().NotBeNull();
    }

    [Fact]
    public void SqlInjectionRule_ignores_normal_url()
    {
        Analyze(new SqlInjectionRule(), Event(url: "/products?id=1")).Should().BeNull();
    }

    [Fact]
    public void XssRule_detects_script_payload()
    {
        Analyze(new XssRule(), Event(url: "/search?q=<script>alert(1)</script>")).Should().NotBeNull();
    }

    [Fact]
    public void XssRule_ignores_plain_search()
    {
        Analyze(new XssRule(), Event(url: "/search?q=karakol")).Should().BeNull();
    }

    [Fact]
    public void PathTraversalRule_detects_etc_passwd()
    {
        Analyze(new PathTraversalRule(), Event(url: "/download?file=../../../../etc/passwd")).Should().NotBeNull();
    }

    [Fact]
    public void PathTraversalRule_detects_double_encoded_traversal()
    {
        Analyze(new PathTraversalRule(), Event(url: "/download?file=%252e%252e%252fetc%252fpasswd")).Should().NotBeNull();
    }

    [Fact]
    public void PathTraversalRule_ignores_normal_download()
    {
        Analyze(new PathTraversalRule(), Event(url: "/download?file=report.pdf")).Should().BeNull();
    }

    [Fact]
    public void ScannerBotRule_detects_wp_admin_path()
    {
        Analyze(new ScannerBotRule(), Event(url: "/wp-admin")).Should().NotBeNull();
    }

    [Fact]
    public void ScannerBotRule_detects_sqlmap_user_agent()
    {
        Analyze(new ScannerBotRule(), Event(url: "/home", userAgent: "sqlmap/1.7")).Should().NotBeNull();
    }

    [Fact]
    public void SensitiveFileAccessRule_detects_env_file()
    {
        Analyze(new SensitiveFileAccessRule(), Event(url: "/.env")).Should().NotBeNull();
    }

    [Fact]
    public void SuspiciousUserAgentRule_detects_empty_user_agent()
    {
        Analyze(new SuspiciousUserAgentRule(), Event(url: "/home", userAgent: null)).Should().NotBeNull();
    }

    [Fact]
    public void SuspiciousUserAgentRule_ignores_browser_user_agent()
    {
        Analyze(new SuspiciousUserAgentRule(), Event(url: "/home", userAgent: "Mozilla/5.0")).Should().BeNull();
    }

    [Fact]
    public void CommandInjectionRule_detects_shell_payload()
    {
        Analyze(new CommandInjectionRule(), Event(url: "/run?cmd=whoami&&cat /etc/passwd")).Should().NotBeNull();
    }

    [Fact]
    public void Detection_finding_contains_standard_evidence_and_action()
    {
        var finding = Analyze(new SqlInjectionRule(), Event(url: "/login?id=1 union select password"));

        finding.Should().NotBeNull();
        finding!.Evidence.Should().ContainSingle();
        finding.Evidence.Single().MatchedPattern.Should().Be("union select");
        finding.Reason.Should().Contain("union select");
        finding.RecommendedAction.Should().Contain("parameterized queries");
    }

    [Fact]
    public void Rule_can_be_disabled_by_rule_id()
    {
        var rule = new SqlInjectionRule();
        var finding = rule.Analyze(Event(url: "/login?id=1 union select password"), new RuleExecutionContext(new RuleOptions(true, [rule.RuleId])));

        finding.Should().BeNull();
    }

    [Fact]
    public void Rule_severity_can_be_overridden_by_rule_id()
    {
        var rule = new SqlInjectionRule();
        var options = new RuleOptions(SeverityOverrides: new Dictionary<string, Severity>
        {
            [rule.RuleId] = Severity.Critical
        });

        var finding = rule.Analyze(Event(url: "/login?id=1 union select password"), new RuleExecutionContext(options));

        finding.Should().NotBeNull();
        finding!.Severity.Should().Be(Severity.Critical);
    }

    private static Karakol.Domain.Findings.DetectionFinding? Analyze(IDetectionRule rule, SecurityEvent securityEvent)
    {
        return rule.Analyze(securityEvent, new RuleExecutionContext(new RuleOptions()));
    }

    private static SecurityEvent Event(string url, string? userAgent = "Mozilla/5.0")
    {
        var path = url.Split('?', 2)[0];
        var query = url.Contains('?', StringComparison.Ordinal) ? url.Split('?', 2)[1] : null;

        return new SecurityEvent(SecurityEventId.New(), LogSourceId.New(), LogFormat.NginxAccess, $"GET {url}", 1)
        {
            SourceIp = "1.2.3.4",
            Url = url,
            Path = path,
            QueryString = query,
            UserAgent = userAgent,
            NormalizedMessage = Uri.UnescapeDataString(url).ToLowerInvariant()
        };
    }
}
