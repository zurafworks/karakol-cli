using Karakol.Shared.Results;
using MediatR;

namespace Karakol.Application.Commands.ScanLogFile;

public sealed record ScanLogFileCommand(
    string FilePath,
    string? Format,
    string OutputDirectory,
    IReadOnlyCollection<string> ReportFormats,
    bool EnableRules,
    bool EnableMl,
    bool EnableCorrelation,
    bool PersistHistory,
    string? MinimumSeverity,
    int? MaxLines,
    bool MaskSensitiveData,
    bool IncludeRawSamples,
    string? ConfigurationFile,
    string? PolicyFile) : IRequest<Result<ScanLogFileResult>>;
