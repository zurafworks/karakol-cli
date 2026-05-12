using Karakol.ML.Abstractions;
using Karakol.ML.Models;

namespace Karakol.ML.Onnx;

public sealed class DisabledOnnxThreatClassifierFactory : IOnnxThreatClassifierFactory
{
    public bool CanCreate(string modelPath, string? metadataPath)
    {
        return File.Exists(modelPath);
    }

    public async Task<(IThreatClassifier? Classifier, ModelMetadata? Metadata)> TryCreateAsync(string modelPath, string? metadataPath, CancellationToken cancellationToken)
    {
        var metadata = metadataPath is null ? null : await ModelMetadataReader.ReadAsync(metadataPath, cancellationToken).ConfigureAwait(false);
        return (null, metadata);
    }
}
