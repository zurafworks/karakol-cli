namespace Karakol.Persistence.Abstractions;

public interface IScanHistoryRepositoryProvider
{
    string ProviderName { get; }

    bool CanCreate(ScanHistoryPersistenceOptions options);

    IScanHistoryRepository Create(ScanHistoryPersistenceOptions options);
}
