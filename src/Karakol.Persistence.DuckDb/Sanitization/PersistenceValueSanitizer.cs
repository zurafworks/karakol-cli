using System.Text.RegularExpressions;

namespace Karakol.Persistence.DuckDb.Sanitization;

internal static partial class PersistenceValueSanitizer
{
    public static string? Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var masked = SensitiveQueryParameterRegex().Replace(value, "$1=******");
        masked = BearerTokenRegex().Replace(masked, "Bearer ******");
        masked = LongTokenRegex().Replace(masked, "******");
        return masked;
    }

    [GeneratedRegex(@"(?i)\b(password|passwd|pwd|token|api_key|apikey|session|sid|secret)=([^&\s]+)")]
    private static partial Regex SensitiveQueryParameterRegex();

    [GeneratedRegex(@"(?i)\bBearer\s+[A-Za-z0-9._~+/=-]{12,}")]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(@"\b[A-Za-z0-9_-]{32,}\b")]
    private static partial Regex LongTokenRegex();
}
