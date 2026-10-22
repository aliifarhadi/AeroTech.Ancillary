using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record GroupedResolvedSelectionSnapshot(
        AncillaryProfile Profile,
        string SelectionId,
        SelectionResolutionStatus Status,
        IReadOnlyList<string> ReasonCodes,
        SelectionLineSnapshot? Line,
        AncillaryShoppingContext? Context);
}
