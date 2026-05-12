using Karakol.Domain.Events;

namespace Karakol.Application.Scanning;

internal static class ScanEventPreprocessor
{
    public static SecurityEvent Preprocess(SecurityEvent securityEvent)
    {
        var normalizedRaw = SafeDecode(securityEvent.RawMessage).ToLowerInvariant();
        var decodedUrl = securityEvent.Url is null ? null : SafeDecode(securityEvent.Url);
        var decodedPath = securityEvent.Path is null ? null : SafeDecode(securityEvent.Path).Replace('\\', '/').ToLowerInvariant();
        var decodedQuery = securityEvent.QueryString is null ? null : SafeDecode(securityEvent.QueryString).ToLowerInvariant();

        return securityEvent with
        {
            Url = decodedUrl ?? securityEvent.Url,
            Path = decodedPath ?? securityEvent.Path,
            QueryString = decodedQuery ?? securityEvent.QueryString,
            NormalizedMessage = string.Join(' ', new[] { normalizedRaw, decodedUrl, decodedPath, decodedQuery, securityEvent.UserAgent }.Where(value => !string.IsNullOrWhiteSpace(value))).ToLowerInvariant()
        };
    }

    private static string SafeDecode(string value)
    {
        try
        {
            return Uri.UnescapeDataString(Uri.UnescapeDataString(value));
        }
        catch (UriFormatException)
        {
            return value;
        }
    }
}
