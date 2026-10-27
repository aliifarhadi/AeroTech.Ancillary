namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record OrderAncillaryOffersSnapshot(long OrderId, int OrderCommercialVersion, AncillaryOffersSnapshot Offers);
}
