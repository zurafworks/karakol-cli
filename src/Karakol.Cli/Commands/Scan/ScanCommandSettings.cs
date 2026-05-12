using Karakol.Application.Commands.ScanLogFile;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Karakol.Cli.Commands.Scan;

public sealed class ScanCommandSettings : CommandSettings
{
    [CommandArgument(0, "<FILE>")]
    public string FilePath { get; init; } = string.Empty;

    [CommandOption("--format")]
    [DefaultValue("auto")]
    public string Format { get; init; } = "auto";

    [CommandOption("--output")]
    [DefaultValue("./reports")]
    public string OutputDirectory { get; init; } = "./reports";

    [CommandOption("--report")]
    public string[] ReportFormats { get; init; } = [];

    [CommandOption("--enable-ml")]
    public bool EnableMl { get; init; }

    [CommandOption("--disable-rules")]
    public bool DisableRules { get; init; }

    [CommandOption("--disable-correlation")]
    public bool DisableCorrelation { get; init; }

    [CommandOption("--persist-history")]
    public bool PersistHistory { get; init; }

    [CommandOption("--mask-sensitive-data")]
    [DefaultValue(true)]
    public bool MaskSensitiveData { get; init; } = true;

    [CommandOption("--include-raw-samples")]
    public bool IncludeRawSamples { get; init; }

    [CommandOption("--min-severity")]
    public string? MinimumSeverity { get; init; }

    [CommandOption("--max-lines")]
    public int? MaxLines { get; init; }

    [CommandOption("--config")]
    public string? ConfigurationFile { get; init; }

    [CommandOption("--policy")]
    public string? PolicyFile { get; init; }

    [CommandOption("--verbose")]
    public bool Verbose { get; init; }

    [CommandOption("--quiet")]
    public bool Quiet { get; init; }

    public ScanLogFileCommand ToCommand()
    {
        return new ScanLogFileCommand(
            FilePath,
            Format,
            OutputDirectory,
            ReportFormats,
            !DisableRules,
            EnableMl,
            !DisableCorrelation,
            PersistHistory,
            MinimumSeverity,
            MaxLines,
            MaskSensitiveData,
            IncludeRawSamples,
            ConfigurationFile,
            PolicyFile);
    }
}
