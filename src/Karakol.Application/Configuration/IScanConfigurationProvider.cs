using Karakol.Shared.Results;

namespace Karakol.Application.Configuration;

public interface IScanConfigurationProvider
{
    Task<Result<KarakolSettings>> LoadAsync(string? configurationFile, CancellationToken cancellationToken);
}
