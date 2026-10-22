using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record GroupedAncillaryOffersSnapshot(
        AncillaryShoppingSnapshot Shopping,
        string? OfferId,
        long? OrderId,
        ShoppingStage ShoppingStage,
        IReadOnlyList<long> FlightIds,
        IReadOnlyList<GroupedOfferItem> Items);
}
