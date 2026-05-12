using System.Text.Json;
using Karakol.Application.Validation;
using Karakol.Shared.Errors;
using Karakol.Shared.Results;

namespace Karakol.Application.Configuration;

public sealed class JsonScanConfigurationProvider : IScanConfigurationProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Result<KarakolSettings>> LoadAsync(string? configurationFile, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(configurationFile))
        {
            return Result<KarakolSettings>.Success(KarakolSettings.Defaults);
        }

        try
        {
            await using var stream = File.OpenRead(configurationFile);
            var settings = await JsonSerializer.DeserializeAsync<KarakolSettings>(stream, SerializerOptions, cancellationToken).ConfigureAwait(false);
            return Validate(settings ?? KarakolSettings.Defaults);
        }
        catch (JsonException ex)
        {
            return Result<KarakolSettings>.Failure(new Error("Config.InvalidJson", "Configuration file is not valid JSON.", ex.Message, ErrorType.Configuration));
        }
        catch (IOException ex)
        {
            return Result<KarakolSettings>.Failure(new Error("Config.ReadFailed", "Configuration file could not be read.", ex.Message, ErrorType.Configuration));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<KarakolSettings>.Failure(new Error("Config.Unauthorized", "Configuration file could not be accessed.", ex.Message, ErrorType.Unauthorized));
        }
    }

    private static Result<KarakolSettings> Validate(KarakolSettings settings)
    {
        if (!KarakolInputValues.IsValidOutputDirectory(settings.Reporting.DefaultOutputDirectory))
        {
            return Result<KarakolSettings>.Failure(new Error(
                "Config.InvalidOutputDirectory",
                "Configuration reporting.defaultOutputDirectory must be a valid directory path and cannot point to an existing file.",
                settings.Reporting.DefaultOutputDirectory,
                ErrorType.Configuration));
        }

        var unknownFormats = KarakolInputValues.FindUnknownReportFormats(settings.Reporting.DefaultFormats);
        if (unknownFormats.Count > 0)
        {
            return Result<KarakolSettings>.Failure(new Error(
                "Config.InvalidReportFormat",
                "Configuration reporting.defaultFormats contains unsupported report formats.",
                string.Join(", ", unknownFormats),
                ErrorType.Configuration));
        }

        return Result<KarakolSettings>.Success(settings);
    }
}
