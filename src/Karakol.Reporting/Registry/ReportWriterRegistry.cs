using Karakol.Reporting.Abstractions;

namespace Karakol.Reporting.Registry;

public sealed class ReportWriterRegistry(IEnumerable<IReportWriter> writers)
{
    private readonly IReadOnlyCollection<IReportWriter> _writers = writers.ToArray();

    public IReportWriter? Resolve(string format) => _writers.FirstOrDefault(writer => string.Equals(writer.Format, format, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyCollection<IReportWriter> GetWriters() => _writers;
}
