using Karakol.Application.Commands.ScanLogFile;
using Karakol.Cli.ExitCodes;
using Karakol.Cli.Logging;
using Karakol.Cli.Rendering;
using MediatR;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Karakol.Cli.Commands.Scan;

public sealed class ScanCommand(IMediator mediator, KarakolLogLevelSwitch logLevelSwitch) : AsyncCommand<ScanCommandSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, ScanCommandSettings settings, CancellationToken cancellationToken)
    {
        logLevelSwitch.Configure(settings.Verbose, settings.Quiet);

        var command = settings.ToCommand();
        var result = settings.Quiet
            ? await mediator.Send(command, cancellationToken).ConfigureAwait(false)
            : await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Analyzing log file...", _ => mediator.Send(command, cancellationToken))
                .ConfigureAwait(false);
        if (result.IsFailure || result.Value is null)
        {
            var error = result.Error;
            Console.Error.WriteLine($"{error?.Code}: {error?.Message}");
            return CliExitCodeMapper.FromError(error);
        }

        if (!settings.Quiet)
        {
            ScanSummaryRenderer.Render(result.Value, settings.Verbose);
        }

        return 0;
    }
}
