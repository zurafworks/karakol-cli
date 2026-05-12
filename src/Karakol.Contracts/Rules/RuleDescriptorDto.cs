namespace Karakol.Contracts.Rules;

public sealed record RuleDescriptorDto(string RuleId, string Name, string Category, string Severity, bool Enabled, string Description);
