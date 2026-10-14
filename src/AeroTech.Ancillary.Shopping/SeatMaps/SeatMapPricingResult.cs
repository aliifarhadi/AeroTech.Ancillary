using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Shopping.SeatMaps
{
    public sealed record SeatMapPricingResult(
        IReadOnlyList<SeatPricing> Seats,
        IReadOnlyList<PublishedRateOfExchange> RatesOfExchange);
}
