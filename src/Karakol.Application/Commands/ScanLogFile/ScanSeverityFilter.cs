using Karakol.Domain.Enums;

namespace Karakol.Application.Commands.ScanLogFile;

internal static class ScanSeverityFilter
{
    public static bool Passes(Severity severity, string? minimumSeverity)
    {
        if (string.IsNullOrWhiteSpace(minimumSeverity))
        {
            return true;
        }

        var minimum = minimumSeverity.Trim().ToLowerInvariant() switch
        {
            "info" => Severity.Info,
            "low" => Severity.Low,
            "medium" => Severity.Medium,
            "high" => Severity.High,
            "critical" => Severity.Critical,
            _ => Severity.Info
        };

        return severity >= minimum;
    }
}
