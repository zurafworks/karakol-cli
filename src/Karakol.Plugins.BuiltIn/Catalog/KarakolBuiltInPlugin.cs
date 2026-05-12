using Karakol.Detection.Abstractions;
using Karakol.Detection.Rules.Injection;
using Karakol.Detection.Rules.Recon;
using Karakol.Detection.Rules.SensitiveData;
using Karakol.Detection.Rules.Web;
using Karakol.ML.Abstractions;
using Karakol.ML.Dummy;
using Karakol.Parsing.Abstractions;
using Karakol.Parsing.Parsers.AccessLogs;
using Karakol.Parsing.Parsers.AuthLogs;
using Karakol.Parsing.Parsers.Generic;
using Karakol.Parsing.Parsers.Structured;
using Karakol.Plugins.Abstractions.Core;
using Karakol.Plugins.Abstractions.Detection;
using Karakol.Plugins.Abstractions.ML;
using Karakol.Plugins.Abstractions.Parsers;
using Karakol.Plugins.Abstractions.Reporting;
using Karakol.Reporting.Abstractions;
using Karakol.Reporting.Sanitization;
using Karakol.Reporting.Writers.Html;
using Karakol.Reporting.Writers.Json;
using Karakol.Reporting.Writers.Markdown;

namespace Karakol.Plugins.BuiltIn.Catalog;

public sealed class KarakolBuiltInPlugin : IParserPlugin, IDetectionPlugin, IReporterPlugin, IMlPlugin
{
    public PluginMetadata Metadata { get; } = new("karakol.builtin", "Karakol Built-in Plugin", "0.1.0", "Built-in parsers, rules, reporters, and dummy ML classifier.");

    public IReadOnlyCollection<ILogParser> GetParsers() =>
    [
        new NginxAccessLogParser(),
        new ApacheAccessLogParser(),
        new AuthLogParser(),
        new SshAuthLogParser(),
        new JsonSecurityEventParser(),
        new CsvSecurityEventParser(),
        new GenericRegexLogParser()
    ];

    public IReadOnlyCollection<IDetectionRule> GetRules() =>
    [
        new SqlInjectionRule(),
        new XssRule(),
        new PathTraversalRule(),
        new ScannerBotRule(),
        new SensitiveFileAccessRule(),
        new SuspiciousUserAgentRule(),
        new CommandInjectionRule()
    ];

    public IReadOnlyCollection<IReportWriter> GetReportWriters()
    {
        var masker = new SensitiveDataMasker();
        return
        [
            new JsonReportWriter(masker),
            new MarkdownReportWriter(),
            new HtmlReportWriter(masker)
        ];
    }

    public IThreatClassifier? CreateClassifier() => new DummyThreatClassifier();
}
