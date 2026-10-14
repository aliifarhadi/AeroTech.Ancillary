using AeroTech.Ancillary.Query.Shopping.Common;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public interface IAncillaryOffersQuery
    {
        string? OfferId { get; }
    }

    public interface IAncillaryOfferItemQuery : IAncillaryOffersQuery
    {
        string? OfferItemRef { get; }
    }
}
