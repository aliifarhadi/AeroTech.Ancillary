namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record FlightOfferFlight(
        int Sequence,
        long FlightCapacityId,
        long FlightId,
        int FlightVersion,
        string? FlightNumber,
        int OriginAirportId,
        int? OriginAirportTerminalId,
        int DestinationAirportId,
        int? DestinationAirportTerminalId,
        int OperatingAirlineId,
        int MarketingAirlineId,
        DateTimeOffset DepartureAt,
        DateTimeOffset ArrivalAt,
        int? AircraftId,
        int? CabinClassId,
        long? RbdId,
        string? BookingClass,
        IReadOnlyList<FlightOfferLeg> Legs);
}
