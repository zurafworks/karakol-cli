using System.Text.RegularExpressions;
using Karakol.Reporting.Abstractions;

namespace Karakol.Reporting.Sanitization;

public sealed class SensitiveDataMasker : ISensitiveDataMasker
{
    private static readonly Regex QuerySecretRegex = new("(?<key>password|passwd|pwd|token|session|sessionid|api_key|apikey|access_token)=([^&\\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BearerRegex = new("Bearer\\s+[A-Za-z0-9._~+/=-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex JwtRegex = new("\\b[A-Za-z0-9_-]{10,}\\.[A-Za-z0-9_-]{10,}\\.[A-Za-z0-9_-]{10,}\\b", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new("\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,}\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Mask(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var masked = QuerySecretRegex.Replace(input, match => $"{match.Groups["key"].Value}=******");
        masked = BearerRegex.Replace(masked, "Bearer ******");
        masked = JwtRegex.Replace(masked, "******.******.******");
        masked = EmailRegex.Replace(masked, "******@******");
        return masked;
    }
}
