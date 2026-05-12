using Karakol.Contracts.Scan;
using Karakol.Domain.Scans;
using Karakol.Reporting.Results;

namespace Karakol.Application.Commands.ScanLogFile;

public sealed record ScanLogFileResult(ScanReport Report, IReadOnlyCollection<ReportWriteResult> ReportFiles, ScanReportDto Dto);
