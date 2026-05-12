using Karakol.Persistence.Abstractions;

namespace Karakol.Persistence.Disabled;

public sealed class DisabledScanHistoryRepositoryProvider : IScanHistoryRepositoryProvider
{
    public string ProviderName => "Disabled";

    public bool CanCreate(ScanHistoryPersistenceOptions options)
    {
        return !options.Enabled;
    }

    public IScanHistoryRepository Create(ScanHistoryPersistenceOptions options)
    {
        return new DisabledScanHistoryRepository();
    }
}
