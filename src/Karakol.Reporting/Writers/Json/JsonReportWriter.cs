using System.Text.Json;
using Karakol.Domain.Scans;
using Karakol.Reporting.Abstractions;
using Karakol.Reporting.Contracts;
using Karakol.Reporting.Formats;
using Karakol.Reporting.Options;
using Karakol.Reporting.Results;

namespace Karakol.Reporting.Writers.Json;

public sealed class JsonReportWriter : IReportWriter
{
    private readonly ISensitiveDataMasker? _masker;

    public JsonReportWriter(ISensitiveDataMasker? masker = null)
    {
        _masker = masker;
    }

    public string Format => ReportFormatNames.Json;
    public string FileExtension => ".json";

    public async Task<ReportWriteResult> WriteAsync(ScanReport report, ReportOptions options, CancellationToken cancellationToken)
    {
        var outputDirectory = NormalizeOutputDirectory(options.OutputDirectory);
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, $"karakol-report-{report.ScanId.Value:N}.json");
        var json = JsonSerializer.Serialize(ReportDocumentProjector.Project(report), new JsonSerializerOptions { WriteIndented = true });
        if (options.MaskSensitiveData && _masker is not null)
        {
            json = _masker.Mask(json);
        }

        await File.WriteAllTextAsync(path, json, cancellationToken).ConfigureAwait(false);
        var fileInfo = new FileInfo(path);
        return new ReportWriteResult(path, Format, fileInfo.Length);
    }

    private static string NormalizeOutputDirectory(string outputDirectory)
    {
        var fullPath = Path.GetFullPath(outputDirectory);
        if (File.Exists(fullPath))
        {
            throw new IOException($"Output path '{outputDirectory}' is an existing file.");
        }

        return fullPath;
    }
}
