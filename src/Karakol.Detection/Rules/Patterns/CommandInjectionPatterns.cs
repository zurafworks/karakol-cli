namespace Karakol.Detection.Rules.Patterns;

internal static class CommandInjectionPatterns
{
    public static readonly string[] Basic =
    [
        ";cat ",
        "|cat ",
        "&&",
        "||",
        "`",
        "$(",
        "bash -c",
        "cmd.exe",
        "powershell",
        "wget http",
        "curl http"
    ];
}
