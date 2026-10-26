using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;

namespace AeroTech.Ancillary.Shopping.Reading
{
    public interface ISelectionSnapshotReader
    {
        Task<SelectionSnapshot?> FindAsync(string id, CancellationToken cancellationToken = default);
    }
}
