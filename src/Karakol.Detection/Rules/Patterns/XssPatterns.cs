namespace Karakol.Detection.Rules.Patterns;

internal static class XssPatterns
{
    public static readonly string[] Basic =
    [
        "<script",
        "</script>",
        "javascript:",
        "onerror=",
        "onload=",
        "alert(",
        "document.cookie",
        "%3cscript",
        "svg/onload",
        "<iframe",
        "eval("
    ];
}
