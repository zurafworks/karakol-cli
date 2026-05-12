using Karakol.Reporting.Formats;

namespace Karakol.Application.Commands.ScanLogFile;

internal static class ScanReportFormatNormalizer
{
    public static IReadOnlyCollection<string> Normalize(IReadOnlyCollection<string> formats)
    {
        return formats.Count == 0
            ? [ReportFormatNames.Json]
            : formats.Select(format => format.Trim().ToLowerInvariant()).Where(format => format.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
