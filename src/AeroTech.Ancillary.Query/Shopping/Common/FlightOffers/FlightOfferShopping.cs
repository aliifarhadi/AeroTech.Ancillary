using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Query.Shopping.Common.FlightOffers
{
    public sealed record FlightOfferShopping(
        FlightOffer Offer,
        FlightOfferFacts Facts,
        int CurrencyId,
        IReadOnlyList<PointOfSaleContext> PointsOfSale);

    public sealed record PointOfSaleContext(long PointOfSaleId, AncillaryShoppingContext Context);
}
