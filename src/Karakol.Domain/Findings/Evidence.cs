namespace Karakol.Domain.Findings;

public sealed record Evidence(string Field, string? Value, string? MatchedPattern, string Explanation);
