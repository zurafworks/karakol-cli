using Karakol.ML.Abstractions;
using Karakol.Plugins.Abstractions.Core;

namespace Karakol.Plugins.Abstractions.ML;

public interface IMlPlugin : IPlugin
{
    IThreatClassifier? CreateClassifier();
}
