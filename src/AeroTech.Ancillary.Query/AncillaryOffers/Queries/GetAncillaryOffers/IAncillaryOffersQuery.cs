namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers
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
