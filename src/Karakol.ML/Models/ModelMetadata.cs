namespace Karakol.ML.Models;

public sealed record ModelMetadata(string ModelName, string ModelVersion, string? Source, DateTimeOffset? CreatedAt);
