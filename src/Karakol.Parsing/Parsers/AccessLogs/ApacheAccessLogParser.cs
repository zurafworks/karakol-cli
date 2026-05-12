using Karakol.Domain.Enums;

namespace Karakol.Parsing.Parsers.AccessLogs;

public sealed class ApacheAccessLogParser : AccessLogParserBase
{
    public override string FormatName => "apache";

    public override LogFormat Format => LogFormat.ApacheAccess;

    public override int Priority => 90;
}
