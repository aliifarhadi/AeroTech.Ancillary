using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Selections;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Options
{
    public interface ITypedSelectionService
    {
        Task<SignedSelection> ReadReferenceAsync(string? reference);

        Task<SelectionOptionsForm> FormAsync(
            SignedSelection reference,
            AncillaryProfile family,
            IReadOnlyList<AncillaryShoppingContext> contexts,
            CancellationToken cancellationToken = default);

        Task<TypedSelectionProof> ProveAsync(
            SignedSelection reference,
            AncillaryProfile family,
            AncillarySelection selection,
            IReadOnlyList<AncillaryShoppingContext> contexts,
            CancellationToken cancellationToken = default);
    }
}
