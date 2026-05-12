using Karakol.Detection.Abstractions;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Karakol.Cli.Commands.Rules;

public sealed class RulesCommand(IRuleRegistry ruleRegistry) : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var table = new Table().RoundedBorder();
        table.AddColumn("RuleId");
        table.AddColumn("Category");
        table.AddColumn("Severity");
        table.AddColumn("Enabled");

        foreach (var rule in ruleRegistry.GetRules().OrderBy(rule => rule.RuleId))
        {
            table.AddRow(rule.RuleId, rule.Category.ToString(), rule.DefaultSeverity.ToString(), "true");
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
