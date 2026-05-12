namespace Karakol.Reporting.Formats;

public static class ReportFormatNames
{
    public const string Json = "json";
    public const string Html = "html";
    public const string Markdown = "markdown";

    public static readonly IReadOnlySet<string> Supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Json,
        Html,
        Markdown
    };
}
