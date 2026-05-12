using Karakol.Cli.Commands.Doctor;
using Karakol.Cli.Commands.Formats;
using Karakol.Cli.Commands.Rules;
using Karakol.Cli.Commands.Scan;
using Karakol.Cli.Commands.Version;
using Karakol.Cli.Composition;
using Karakol.Cli.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console.Cli;

var services = new ServiceCollection();
var logLevelSwitch = new KarakolLogLevelSwitch();
KarakolRuntime.ConfigureServices(services, logLevelSwitch);

var app = new CommandApp(new TypeRegistrar(services));
app.Configure(configuration =>
{
    configuration.SetApplicationName("karakol");
    configuration.AddCommand<ScanCommand>("scan")
        .WithDescription("Analyze a log file and generate terminal, JSON, Markdown or HTML reports.");
    configuration.AddCommand<RulesCommand>("rules")
        .WithDescription("List built-in detection rules.");
    configuration.AddCommand<FormatsCommand>("formats")
        .WithDescription("List supported log formats.");
    configuration.AddCommand<DoctorCommand>("doctor")
        .WithDescription("Run local environment checks.");
    configuration.AddCommand<VersionCommand>("version")
        .WithDescription("Show Karakol version.");
});

try
{
    return await app.RunAsync(args).ConfigureAwait(false);
}
finally
{
    Log.CloseAndFlush();
}
