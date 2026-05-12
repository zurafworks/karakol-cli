using Karakol.Domain.Sources;

namespace Karakol.Parsing.Options;

public sealed record ParserOptions(LogSource Source, IReadOnlyDictionary<string, string>? ColumnMappings = null);
