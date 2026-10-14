using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Query.Shopping.Common.FlightOffers
{
    public sealed record FlightOfferFacts(
        IReadOnlyList<TravellerFareFacts> Fares,
        IReadOnlyList<TravellerFlightBaggageFacts> Baggage,
        FactCompleteness Completeness);
}
