using Karakol.Domain.Enums;
using Karakol.Reporting.Formats;

namespace Karakol.Application.Validation;

public static class KarakolInputValues
{
    public static bool IsKnownSeverity(string? value)
    {
        return string.IsNullOrWhiteSpace(value) || Enum.TryParse<Severity>(value.Trim(), ignoreCase: true, out _);
    }

    public static bool IsKnownReportFormat(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return ReportFormatNames.Supported.Contains(normalized);
    }

    public static IReadOnlyCollection<string> FindUnknownReportFormats(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Where(value => !IsKnownReportFormat(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static bool IsValidOutputDirectory(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            var fullPath = Path.GetFullPath(value);
            return !File.Exists(fullPath);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (PathTooLongException)
        {
            return false;
        }
    }
}
