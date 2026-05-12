using Karakol.Plugins.Abstractions.Core;
using Karakol.Reporting.Abstractions;

namespace Karakol.Plugins.Abstractions.Reporting;

public interface IReporterPlugin : IPlugin
{
    IReadOnlyCollection<IReportWriter> GetReportWriters();
}
