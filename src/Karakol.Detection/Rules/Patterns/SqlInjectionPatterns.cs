namespace Karakol.Detection.Rules.Patterns;

internal static class SqlInjectionPatterns
{
    public static readonly string[] Basic =
    [
        "' or '1'='1",
        "or 1=1",
        "union select",
        "information_schema",
        "sleep(",
        "benchmark(",
        "drop table",
        "xp_cmdshell",
        "concat(",
        "load_file(",
        "waitfor delay",
        "select * from",
        "insert into",
        "delete from"
    ];
}
