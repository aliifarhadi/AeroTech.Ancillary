using AeroTech.Ancillary.Query.Shopping.Common;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public interface IAncillaryOffersQuery
    {
        string? OfferId { get; }

        long? OrderId { get; }
    }

    public interface IAncillaryOfferItemQuery : IAncillaryOffersQuery
    {
        string? OfferItemRef { get; }
    }
}
