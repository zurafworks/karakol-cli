using Karakol.Domain.Scans;
using Karakol.Reporting.Options;
using Karakol.Reporting.Results;

namespace Karakol.Reporting.Abstractions;

public interface IReportWriter
{
    string Format { get; }
    string FileExtension { get; }
    Task<ReportWriteResult> WriteAsync(ScanReport report, ReportOptions options, CancellationToken cancellationToken);
}
