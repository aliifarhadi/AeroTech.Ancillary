namespace AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Contracts
{
    public interface ISelectionSnapshotRepository
    {
        Task AddAsync(SelectionSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
