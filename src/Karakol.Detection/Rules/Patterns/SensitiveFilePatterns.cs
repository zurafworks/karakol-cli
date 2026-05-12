namespace Karakol.Detection.Rules.Patterns;

internal static class SensitiveFilePatterns
{
    public static readonly string[] Basic =
    [
        ".env",
        "id_rsa",
        "config.yml",
        "appsettings.json",
        "web.config",
        ".git/config",
        "backup.zip",
        "backup.sql",
        "database.sql",
        "dump.sql",
        ".npmrc",
        ".aws/credentials"
    ];
}
