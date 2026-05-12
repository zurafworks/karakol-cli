using Spectre.Console;
using Spectre.Console.Cli;
using System.Reflection;

namespace Karakol.Cli.Commands.Version;

public sealed class VersionCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var version = typeof(VersionCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(VersionCommand).Assembly.GetName().Version?.ToString(3)
            ?? "unknown";

        AnsiConsole.MarkupLine($"[bold]Karakol[/] {version}");
        return 0;
    }
}
