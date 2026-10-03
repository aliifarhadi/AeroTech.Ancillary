namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public sealed record ReservationContextInput(
        int CurrencyId,
        DateTimeOffset AsOf,
        ReservationSalesContextInput? SalesContext,
        IReadOnlyList<ReservationTravellerInput> Travellers,
        IReadOnlyList<ReservationBoundInput> Bounds,
        IReadOnlyList<ReservationExistingOccurrenceInput>? Existing);

    public sealed record ReservationSalesContextInput(
        string? Channel,
        long? CustomerId,
        long? TravelAgencyId,
        int? CountryId);

    public sealed record ReservationTravellerInput(
        string Ref,
        string PassengerTypeCode,
        IReadOnlyList<string> FlightRefs);

    public sealed record ReservationBoundInput(
        string Ref,
        IReadOnlyList<ReservationFlightInput> Flights);

    public sealed record ReservationFlightInput(
        string Ref,
        long FlightId,
        long? FlightCapacityId,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset DepartureDateTime,
        int MarketingAirlineId,
        int OperatingAirlineId,
        int? AircraftId,
        int? CabinClassId,
        long? RbdId);

    public sealed record ReservationExistingOccurrenceInput(
        string ProductRef,
        string TravellerRef,
        string? BoundRef,
        string? FlightRef,
        int Quantity);

    public sealed record ReservationUnitInput(
        string UnitReference,
        string ProductRef,
        int ProductVersion,
        long PriceRuleId,
        string TravellerRef,
        string? BoundRef,
        string? FlightRef,
        int Quantity);
}
