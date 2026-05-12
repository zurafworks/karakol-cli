using FluentAssertions;
using Karakol.Detection.Abstractions;
using Karakol.Detection.Context;
using Karakol.Detection.Options;
using Karakol.Detection.Rules;
using Karakol.Detection.Rules.Injection;
using Karakol.Detection.Rules.Recon;
using Karakol.Detection.Rules.SensitiveData;
using Karakol.Detection.Rules.Web;
using Karakol.Domain.Enums;
using Karakol.Domain.Events;
using Karakol.Domain.Sources;

namespace Karakol.Detection.Tests;

public sealed class RuleCorpusTests
{
    private static readonly IReadOnlyDictionary<string, IDetectionRule> Rules = new Dictionary<string, IDetectionRule>(StringComparer.OrdinalIgnoreCase)
    {
        [RuleIds.SqlInjectionBasic] = new SqlInjectionRule(),
        [RuleIds.XssBasic] = new XssRule(),
        [RuleIds.PathTraversalBasic] = new PathTraversalRule(),
        [RuleIds.ScannerBotBasic] = new ScannerBotRule(),
        [RuleIds.SensitiveFileBasic] = new SensitiveFileAccessRule(),
        [RuleIds.SuspiciousUserAgentBasic] = new SuspiciousUserAgentRule(),
        [RuleIds.CommandInjectionBasic] = new CommandInjectionRule()
    };

    public static IEnumerable<object[]> CorpusCases()
    {
        var corpusPath = Path.Combine(AppContext.BaseDirectory, "TestData", "rules", "rule-corpus.csv");
        foreach (var line in File.ReadLines(corpusPath).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',', 4);
            yield return [parts[0], parts[1], parts[2], parts[3]];
        }
    }

    [Theory]
    [MemberData(nameof(CorpusCases))]
    public void Rule_corpus_matches_expected_detection_behavior(string ruleId, string kind, string url, string userAgent)
    {
        var rule = Rules[ruleId];
        var finding = rule.Analyze(Event(url, userAgent), new RuleExecutionContext(new RuleOptions()));

        if (string.Equals(kind, "malicious", StringComparison.OrdinalIgnoreCase))
        {
            finding.Should().NotBeNull($"corpus case for {ruleId} should be detected");
            finding!.DetectorId.Should().Be(ruleId);
            finding.Evidence.Should().NotBeEmpty();
            finding.Evidence.Should().OnlyContain(evidence => !string.IsNullOrWhiteSpace(evidence.Field));
            finding.RecommendedAction.Should().NotBeNullOrWhiteSpace();
        }
        else
        {
            finding.Should().BeNull($"corpus case for {ruleId} should not be a false positive");
        }
    }

    private static SecurityEvent Event(string url, string userAgent)
    {
        var decodedUrl = Uri.UnescapeDataString(url);
        var path = decodedUrl.Split('?', 2)[0];
        var query = decodedUrl.Contains('?', StringComparison.Ordinal) ? decodedUrl.Split('?', 2)[1] : null;

        return new SecurityEvent(SecurityEventId.New(), LogSourceId.New(), LogFormat.NginxAccess, $"GET {url}", 1)
        {
            SourceIp = "203.0.113.10",
            Url = decodedUrl,
            Path = path,
            QueryString = query,
            UserAgent = userAgent,
            NormalizedMessage = decodedUrl.ToLowerInvariant()
        };
    }
}
