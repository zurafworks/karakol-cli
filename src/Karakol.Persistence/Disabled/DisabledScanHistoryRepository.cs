using Karakol.Domain.Scans;
using Karakol.Persistence.Abstractions;

namespace Karakol.Persistence.Disabled;

public sealed class DisabledScanHistoryRepository : IScanHistoryRepository
{
    public Task SaveAsync(ScanReport report, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<ScanReport?> GetByIdAsync(ScanId scanId, CancellationToken cancellationToken) => Task.FromResult<ScanReport?>(null);

    public Task<IReadOnlyList<ScanHistoryItem>> ListAsync(ScanHistoryFilter filter, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ScanHistoryItem>>(Array.Empty<ScanHistoryItem>());

    public Task DeleteAsync(ScanId scanId, CancellationToken cancellationToken) => Task.CompletedTask;
}
