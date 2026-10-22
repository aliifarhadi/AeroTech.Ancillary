namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections
{
    public sealed record SeatHoldFlightInput(
        string FlightNumber,
        int MarketingAirlineId,
        int OperatingAirlineId,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset DepartureAt,
        DateTimeOffset ArrivalAt,
        int AircraftId,
        int CabinClassId,
        long? RbdId);
}
