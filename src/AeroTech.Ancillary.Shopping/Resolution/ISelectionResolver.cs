using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Resolution
{
    public interface ISelectionResolver
    {
        Task<SelectionSnapshot> OpenAsync(string selectionId, CancellationToken cancellationToken = default);

        Task<SelectionSnapshot> OpenAsync(string selectionId, ShoppingSourceKind source, CancellationToken cancellationToken = default);

        Task<ResolvedSelections> ResolveAsync(string selectionId, CancellationToken cancellationToken = default);
    }
}
