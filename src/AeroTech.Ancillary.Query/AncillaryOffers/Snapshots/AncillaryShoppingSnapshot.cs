using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Snapshots
{
    public sealed record AncillaryShoppingSnapshot(
        FlightOffer Offer,
        AncillarySalesScope Scope,
        FlightOfferFacts Facts,
        int CurrencyId,
        IReadOnlyList<PointOfSaleShopping> PointsOfSale)
    {
        public IEnumerable<AncillaryOfferItem> Items => PointsOfSale.SelectMany(pointOfSale => pointOfSale.Items);
    }

    public sealed record PointOfSaleShopping(
        long PointOfSaleId,
        AncillaryShoppingContext Context,
        CanonicalAncillaryOfferResult Result,
        IReadOnlyList<AncillaryOfferItem> Items);

    public sealed record AncillaryOfferItem(
        string OfferItemRef,
        long PointOfSaleId,
        AncillaryShoppingContext Context,
        CanonicalAncillaryOfferCandidate Candidate);

    public sealed record FlightOfferFacts(
        IReadOnlyList<TravellerFareFacts> Fares,
        IReadOnlyList<TravellerFlightBaggageFacts> Baggage,
        FactCompleteness Completeness);
}
