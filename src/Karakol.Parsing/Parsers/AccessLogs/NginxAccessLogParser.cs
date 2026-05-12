using Karakol.Domain.Enums;

namespace Karakol.Parsing.Parsers.AccessLogs;

public sealed class NginxAccessLogParser : AccessLogParserBase
{
    public override string FormatName => "nginx";

    public override LogFormat Format => LogFormat.NginxAccess;

    public override int Priority => 100;
}
