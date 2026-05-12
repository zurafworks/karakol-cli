using Karakol.Domain.Enums;

namespace Karakol.Application.Commands.ScanLogFile;

internal static class ScanLogFileFormatParser
{
    public static LogFormat Parse(string? format)
    {
        return string.IsNullOrWhiteSpace(format)
            ? LogFormat.Auto
            : format.Trim().ToLowerInvariant() switch
            {
                "nginx" or "nginxaccess" => LogFormat.NginxAccess,
                "apache" or "apacheaccess" => LogFormat.ApacheAccess,
                "auth" => LogFormat.Auth,
                "ssh" or "sshauth" => LogFormat.SshAuth,
                "json" => LogFormat.Json,
                "csv" => LogFormat.Csv,
                "generic" => LogFormat.Generic,
                "auto" => LogFormat.Auto,
                _ => LogFormat.Unknown
            };
    }
}
