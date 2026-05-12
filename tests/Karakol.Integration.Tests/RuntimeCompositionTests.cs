using FluentAssertions;
using Karakol.Cli.Composition;
using Karakol.Detection.Abstractions;
using Karakol.ML.Abstractions;
using Karakol.Parsing.Abstractions;
using Karakol.Reporting.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Karakol.Integration.Tests;

public sealed class RuntimeCompositionTests
{
    [Fact]
    public void Runtime_uses_built_in_plugin_catalog_for_core_registrations()
    {
        var services = KarakolRuntime.CreateDefault().Services;

        services.GetServices<ILogParser>().Select(parser => parser.FormatName).Should().Contain(["nginx", "apache", "auth", "ssh-auth", "json", "csv", "generic"]);
        services.GetServices<IDetectionRule>().Select(rule => rule.RuleId).Should().Contain("sqli.basic");
        services.GetServices<IReportWriter>().Select(writer => writer.Format).Should().Contain(["json", "html", "markdown"]);
        services.GetRequiredService<IThreatClassifier>().ModelName.Should().Be("dummy");
    }
}
