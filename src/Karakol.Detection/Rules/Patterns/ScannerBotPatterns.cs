namespace Karakol.Detection.Rules.Patterns;

internal static class ScannerBotPatterns
{
    public static readonly string[] Basic =
    [
        "/wp-admin",
        "/wp-login.php",
        "/phpmyadmin",
        "/.env",
        "/server-status",
        "/actuator",
        "/admin",
        "/config.php",
        "/.git/config",
        "/vendor/phpunit",
        "/cgi-bin",
        "/boaform",
        "sqlmap",
        "nikto",
        "masscan",
        "nmap",
        "zgrab",
        "dirbuster",
        "gobuster",
        "nuclei",
        "python-requests",
        "curl",
        "wget"
    ];
}
