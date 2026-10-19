namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record FlightOfferLeg(
        int Sequence,
        long LegId,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset DepartureAt,
        DateTimeOffset ArrivalAt);
}
