namespace Karakol.Detection.Rules.Patterns;

internal static class PathTraversalPatterns
{
    public static readonly string[] Basic =
    [
        "../",
        "..\\",
        "%2e%2e%2f",
        "%252e%252e%252f",
        "/etc/passwd",
        "boot.ini",
        "win.ini",
        "/proc/self/environ",
        "windows/system32"
    ];
}
