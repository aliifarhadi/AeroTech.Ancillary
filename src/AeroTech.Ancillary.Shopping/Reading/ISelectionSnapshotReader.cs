using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;

namespace AeroTech.Ancillary.Shopping.Reading
{
    public interface ISelectionSnapshotReader
    {
        Task<IReadOnlyList<SelectionSnapshot>> FindAsync(IReadOnlyCollection<string> ids, CancellationToken cancellationToken = default);
    }
}
