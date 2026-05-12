using Karakol.ML.Abstractions;
using Karakol.ML.Models;

namespace Karakol.ML.Onnx;

public interface IOnnxThreatClassifierFactory
{
    bool CanCreate(string modelPath, string? metadataPath);

    Task<(IThreatClassifier? Classifier, ModelMetadata? Metadata)> TryCreateAsync(string modelPath, string? metadataPath, CancellationToken cancellationToken);
}
