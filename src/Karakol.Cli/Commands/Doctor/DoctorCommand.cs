using Spectre.Console;
using Spectre.Console.Cli;

namespace Karakol.Cli.Commands.Doctor;

public sealed class DoctorCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        var table = new Table().RoundedBorder();
        table.AddColumn("Check");
        table.AddColumn("Result");
        table.AddRow("Current directory", Directory.GetCurrentDirectory());
        table.AddRow("Reports writable", CanWrite("./reports").ToString());
        table.AddRow("DuckDB history path writable", CanWrite(Path.GetDirectoryName("./reports/karakol-history.duckdb") ?? ".").ToString());
        table.AddRow("Samples present", Directory.Exists("./samples").ToString());
        AnsiConsole.Write(new Panel(table).Header("Karakol doctor"));
        return 0;
    }

    private static bool CanWrite(string directory)
    {
        try
        {
            Directory.CreateDirectory(directory);
            var probe = Path.Combine(directory, $".karakol-write-{Guid.NewGuid():N}.tmp");
            File.WriteAllText(probe, "ok");
            File.Delete(probe);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
