using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record FlightOfferBound(
        string BoundId,
        int Sequence,
        BoundDirection? Direction,
        int OriginAirportId,
        int DestinationAirportId,
        IReadOnlyList<FlightOfferFlight> Flights);
}
