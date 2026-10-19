namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record FlightOfferCoupon(
        string BoundId,
        long FlightId,
        FlightOfferBaggage? CheckedBaggage,
        FlightOfferBaggage? CabinBaggage,
        bool IsRefundable,
        bool IsChangeable,
        bool IsUpgradable,
        IReadOnlyList<long> AirFareIds,
        IReadOnlyList<FlightOfferPriceLine> PriceLines);
}
