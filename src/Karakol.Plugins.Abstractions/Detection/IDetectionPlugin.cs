using Karakol.Detection.Abstractions;
using Karakol.Plugins.Abstractions.Core;

namespace Karakol.Plugins.Abstractions.Detection;

public interface IDetectionPlugin : IPlugin
{
    IReadOnlyCollection<IDetectionRule> GetRules();
}
