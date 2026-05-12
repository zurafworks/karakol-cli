using Karakol.Domain.Scans;
using Karakol.Persistence.Abstractions;

namespace Karakol.Persistence.SQLite;

public sealed class SQLiteScanHistoryRepository(SQLitePersistenceOptions options) : IScanHistoryRepository
{
    public Task SaveAsync(ScanReport report, CancellationToken cancellationToken)
    {
        ThrowIfDisabled();
        throw new NotImplementedException("SQLite scan history is a post-MVP implementation placeholder.");
    }

    public Task<ScanReport?> GetByIdAsync(ScanId scanId, CancellationToken cancellationToken)
    {
        ThrowIfDisabled();
        throw new NotImplementedException("SQLite scan history is a post-MVP implementation placeholder.");
    }

    public Task<IReadOnlyList<ScanHistoryItem>> ListAsync(ScanHistoryFilter filter, CancellationToken cancellationToken)
    {
        ThrowIfDisabled();
        throw new NotImplementedException("SQLite scan history is a post-MVP implementation placeholder.");
    }

    public Task DeleteAsync(ScanId scanId, CancellationToken cancellationToken)
    {
        ThrowIfDisabled();
        throw new NotImplementedException("SQLite scan history is a post-MVP implementation placeholder.");
    }

    private void ThrowIfDisabled()
    {
        if (!options.Enabled)
        {
            throw new InvalidOperationException("SQLite persistence is disabled. Enable it explicitly in a post-MVP persistence configuration.");
        }
    }
}
