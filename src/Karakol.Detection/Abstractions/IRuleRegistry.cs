namespace Karakol.Detection.Abstractions;

public interface IRuleRegistry
{
    IReadOnlyCollection<IDetectionRule> GetRules();
}
