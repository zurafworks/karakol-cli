using Karakol.Domain.Scans;

namespace Karakol.Persistence.Abstractions;

public interface IScanHistoryRepository
{
    Task SaveAsync(ScanReport report, CancellationToken cancellationToken);

    Task<ScanReport?> GetByIdAsync(ScanId scanId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ScanHistoryItem>> ListAsync(ScanHistoryFilter filter, CancellationToken cancellationToken);

    Task DeleteAsync(ScanId scanId, CancellationToken cancellationToken);
}
