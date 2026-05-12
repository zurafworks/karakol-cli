using System.Text.Json;

namespace Karakol.ML.Models;

public static class ModelMetadataReader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<ModelMetadata?> ReadAsync(string metadataPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(metadataPath))
        {
            return null;
        }

        await using var stream = File.OpenRead(metadataPath);
        return await JsonSerializer.DeserializeAsync<ModelMetadata>(stream, Options, cancellationToken).ConfigureAwait(false);
    }
}
