using FluentValidation;
using Karakol.Application.Validation;
using Karakol.Reporting.Formats;

namespace Karakol.Application.Commands.ScanLogFile;

public sealed class ScanLogFileCommandValidator : AbstractValidator<ScanLogFileCommand>
{
    public ScanLogFileCommandValidator()
    {
        RuleFor(command => command.FilePath)
            .NotEmpty()
            .WithErrorCode("Scan.FilePathRequired")
            .WithMessage("FilePath is required.");

        RuleFor(command => command.FilePath)
            .Must(File.Exists)
            .When(command => !string.IsNullOrWhiteSpace(command.FilePath))
            .WithErrorCode("Scan.FileNotFound")
            .WithMessage(command => $"File '{command.FilePath}' was not found.");

        RuleFor(command => command.MaxLines)
            .Must(maxLines => maxLines is null or > 0)
            .WithErrorCode("Scan.InvalidMaxLines")
            .WithMessage("MaxLines must be positive.");

        RuleFor(command => command.ConfigurationFile)
            .Must(path => string.IsNullOrWhiteSpace(path) || File.Exists(path))
            .WithErrorCode("Scan.ConfigFileNotFound")
            .WithMessage(command => $"Configuration file '{command.ConfigurationFile}' was not found.");

        RuleFor(command => command.PolicyFile)
            .Must(path => string.IsNullOrWhiteSpace(path) || File.Exists(path))
            .WithErrorCode("Scan.PolicyFileNotFound")
            .WithMessage(command => $"Policy file '{command.PolicyFile}' was not found.");

        RuleFor(command => command.MinimumSeverity)
            .Must(KarakolInputValues.IsKnownSeverity)
            .WithErrorCode("Scan.InvalidMinimumSeverity")
            .WithMessage("MinimumSeverity must be one of: info, low, medium, high, critical.");

        RuleFor(command => command.OutputDirectory)
            .Must(KarakolInputValues.IsValidOutputDirectory)
            .WithErrorCode("Scan.InvalidOutputDirectory")
            .WithMessage(command => $"Output directory '{command.OutputDirectory}' is not valid or points to an existing file.");

        RuleFor(command => command)
            .Must(command => ScanReportFormatNormalizer
                .Normalize(command.ReportFormats)
                .All(format => ReportFormatNames.Supported.Contains(format)))
            .WithErrorCode("Scan.InvalidReportFormat")
            .WithMessage("One or more report formats are not supported.");
    }
}
