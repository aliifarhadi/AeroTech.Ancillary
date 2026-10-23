using AeroTech.Ancillary.Shopping.Offers;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record AncillaryOffersSnapshot(string OfferId, int CurrencyId, IReadOnlyList<AncillaryOfferRow> Rows);
}
