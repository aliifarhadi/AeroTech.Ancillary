using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Contracts;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public sealed class SnapshotStore : ISelectionSnapshotRepository, IUnitOfWork
{
    private readonly InMemoryCatalog _catalog;
    private readonly List<SelectionSnapshot> _pending = [];

    public SnapshotStore(InMemoryCatalog catalog) => _catalog = catalog;

    public int Saves { get; private set; }

    public bool FailsOnSave { get; init; }

    public Task AddAsync(SelectionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        _pending.Add(snapshot);

        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        Saves++;

        if (FailsOnSave)
            throw new InvalidOperationException("The database refused the transaction.");

        var saved = _pending.Count;

        _catalog.Snapshots.AddRange(_pending);
        _pending.Clear();

        return Task.FromResult(saved);
    }
}
