namespace AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote
{
    public sealed record QuoteSalesContext(
        string? Channel,
        long? CustomerId,
        long? TravelAgencyId,
        int? CountryId);

    public sealed record QuoteTraveller(
        string Ref,
        string PassengerTypeCode,
        IReadOnlyList<string> FlightRefs);

    public sealed record QuoteBound(
        string Ref,
        IReadOnlyList<QuoteFlight> Flights);

    public sealed record QuoteFlight(
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

    public sealed record QuoteSelection(
        string ProductRef,
        int ProductVersion,
        long PriceRuleId,
        string TravellerRef,
        string? BoundRef,
        string? FlightRef,
        int Quantity);

    public sealed record QuoteExistingOccurrence(
        string ProductRef,
        string TravellerRef,
        string? BoundRef,
        string? FlightRef,
        int Quantity);
}
