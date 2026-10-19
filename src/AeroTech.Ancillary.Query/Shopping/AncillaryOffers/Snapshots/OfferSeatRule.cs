namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record OfferSeatRule(IReadOnlyList<string> SeatNumbers, IReadOnlyList<string> SeatCharacteristicCodes);
}
