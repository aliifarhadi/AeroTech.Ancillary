using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch
{
    public sealed record SelectionLineRequest(
        string? ClientSelectionId,
        SelectionLineKind? Kind,
        string? OfferItemRef,
        AncillarySelection? Selection,
        string? SeatMapSelectionId);
}
