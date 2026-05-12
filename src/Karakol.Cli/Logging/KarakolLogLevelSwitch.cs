using Serilog.Core;
using Serilog.Events;

namespace Karakol.Cli.Logging;

public sealed class KarakolLogLevelSwitch
{
    public KarakolLogLevelSwitch()
    {
        Switch = new LoggingLevelSwitch(LogEventLevel.Warning);
    }

    public LoggingLevelSwitch Switch { get; }

    public void Configure(bool verbose, bool quiet)
    {
        Switch.MinimumLevel = quiet
            ? LogEventLevel.Error
            : verbose
                ? LogEventLevel.Information
                : LogEventLevel.Warning;
    }
}
