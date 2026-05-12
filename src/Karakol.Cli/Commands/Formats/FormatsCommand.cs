using Karakol.Domain.Enums;
using Karakol.Parsing.Abstractions;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Karakol.Cli.Commands.Formats;

public sealed class FormatsCommand(IParserRegistry parserRegistry) : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var table = new Table().RoundedBorder();
        table.AddColumn("Format");
        table.AddColumn("Parser");
        table.AddColumn("AutoDetect");

        foreach (var parser in parserRegistry.GetParsers().OrderBy(parser => parser.Format.ToString()))
        {
            table.AddRow(parser.Format.ToString(), parser.FormatName, (parser.Format != LogFormat.Generic).ToString());
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
