using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record GroupedSelectionResolutionSnapshot(
        string? OfferId,
        long? OrderId,
        int? OrderCommercialVersion,
        SeatMapResolutionSnapshot Seats,
        IReadOnlyList<GroupedResolvedSelectionSnapshot> Selections)
    {
        public bool AllValid => Selections.All(selection => selection.Status == SelectionResolutionStatus.Valid);
    }
}
