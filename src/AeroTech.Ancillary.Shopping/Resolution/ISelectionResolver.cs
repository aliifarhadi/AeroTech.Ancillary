using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;

namespace AeroTech.Ancillary.Shopping.Resolution
{
    public interface ISelectionResolver
    {
        Task<SelectionSnapshot> OpenAsync(string selectionId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ResolvedSelection>> ResolveAsync(string selectionId, CancellationToken cancellationToken = default);
    }
}
