using System.Net;
using Karakol.Domain.Events;

namespace Karakol.Application.Policies;

internal static class PolicyMatcher
{
    public static bool IsIgnored(SecurityEvent securityEvent, PolicyContext policy)
    {
        return MatchesPath(securityEvent.Path ?? securityEvent.Url, policy.IgnoredPaths);
    }

    public static bool IsSensitiveEndpoint(string? targetResource, PolicyContext policy)
    {
        return MatchesPath(targetResource, policy.SensitivePaths);
    }

    public static bool IsTrustedIp(string? sourceIp, PolicyContext policy)
    {
        if (string.IsNullOrWhiteSpace(sourceIp))
        {
            return false;
        }

        foreach (var trustedIp in policy.TrustedIps)
        {
            if (string.Equals(sourceIp, trustedIp, StringComparison.OrdinalIgnoreCase) || MatchesCidr(sourceIp, trustedIp))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesPath(string? value, IReadOnlyCollection<string> patterns)
    {
        if (string.IsNullOrWhiteSpace(value) || patterns.Count == 0)
        {
            return false;
        }

        return patterns.Any(pattern => value.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) || value.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesCidr(string sourceIp, string cidr)
    {
        var parts = cidr.Split('/', 2);
        if (parts.Length != 2 || !IPAddress.TryParse(sourceIp, out var source) || !IPAddress.TryParse(parts[0], out var network) || !int.TryParse(parts[1], out var prefix))
        {
            return false;
        }

        var sourceBytes = source.GetAddressBytes();
        var networkBytes = network.GetAddressBytes();
        if (sourceBytes.Length != networkBytes.Length || prefix < 0 || prefix > sourceBytes.Length * 8)
        {
            return false;
        }

        var fullBytes = prefix / 8;
        var remainingBits = prefix % 8;
        for (var index = 0; index < fullBytes; index++)
        {
            if (sourceBytes[index] != networkBytes[index])
            {
                return false;
            }
        }

        if (remainingBits == 0)
        {
            return true;
        }

        var mask = (byte)~(255 >> remainingBits);
        return (sourceBytes[fullBytes] & mask) == (networkBytes[fullBytes] & mask);
    }
}
