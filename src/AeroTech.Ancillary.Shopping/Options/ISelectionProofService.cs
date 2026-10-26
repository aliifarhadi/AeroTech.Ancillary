using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Selections;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Options
{
    public interface ISelectionProofService
    {
        Task<SignedSelection> ReadAsync(string? id, SelectionIdKind kind);

        Task<SelectionProof> ProveDirectAsync(
            SignedSelection selection,
            int? quantity,
            IReadOnlyList<AncillaryShoppingContext> contexts,
            CancellationToken cancellationToken = default);

        Task<SelectionProof> ProveTypedAsync(
            SignedSelection reference,
            AncillaryProfile family,
            AncillarySelection selection,
            IReadOnlyList<AncillaryShoppingContext> contexts,
            CancellationToken cancellationToken = default);
    }
}
