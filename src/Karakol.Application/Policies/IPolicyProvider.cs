using Karakol.Shared.Results;

namespace Karakol.Application.Policies;

public interface IPolicyProvider
{
    Task<Result<PolicyContext>> LoadAsync(string? policyFile, CancellationToken cancellationToken);
}
