using Karakol.Detection.Abstractions;
using Karakol.ML.Abstractions;
using Karakol.Parsing.Abstractions;
using Karakol.Plugins.Abstractions.Detection;
using Karakol.Plugins.Abstractions.ML;
using Karakol.Plugins.Abstractions.Parsers;
using Karakol.Plugins.Abstractions.Reporting;
using Karakol.Reporting.Abstractions;

namespace Karakol.Plugins.Abstractions.Registry;

public sealed class PluginRegistry
{
    private readonly List<ILogParser> _parsers = [];
    private readonly List<IDetectionRule> _rules = [];
    private readonly List<IReportWriter> _reportWriters = [];
    private readonly List<IThreatClassifier> _classifiers = [];

    public void Register(IParserPlugin plugin) => _parsers.AddRange(plugin.GetParsers());

    public void Register(IDetectionPlugin plugin) => _rules.AddRange(plugin.GetRules());

    public void Register(IReporterPlugin plugin) => _reportWriters.AddRange(plugin.GetReportWriters());

    public void Register(IMlPlugin plugin)
    {
        var classifier = plugin.CreateClassifier();
        if (classifier is not null)
        {
            _classifiers.Add(classifier);
        }
    }

    public IReadOnlyCollection<ILogParser> GetParsers() => _parsers;

    public IReadOnlyCollection<IDetectionRule> GetRules() => _rules;

    public IReadOnlyCollection<IReportWriter> GetReportWriters() => _reportWriters;

    public IReadOnlyCollection<IThreatClassifier> GetClassifiers() => _classifiers;
}
