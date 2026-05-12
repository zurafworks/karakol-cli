using FluentAssertions;
using Karakol.Domain.Scans;
using Karakol.Persistence.Abstractions;
using Karakol.Persistence.Disabled;
using Karakol.Persistence.SQLite;

namespace Karakol.Persistence.Tests;

public sealed class DisabledScanHistoryRepositoryTests
{
    [Fact]
    public async Task Disabled_repository_is_noop_and_returns_empty_results()
    {
        var repository = new DisabledScanHistoryRepository();

        await repository.SaveAsync(null!, CancellationToken.None);
        var item = await repository.GetByIdAsync(ScanId.New(), CancellationToken.None);
        var list = await repository.ListAsync(new ScanHistoryFilter(), CancellationToken.None);

        item.Should().BeNull();
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task SQLite_placeholder_requires_explicit_enablement()
    {
        var repository = new SQLiteScanHistoryRepository(new SQLitePersistenceOptions());

        var act = async () => await repository.ListAsync(new ScanHistoryFilter(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
