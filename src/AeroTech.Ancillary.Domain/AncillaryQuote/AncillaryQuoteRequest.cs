namespace AeroTech.Ancillary.Domain.AncillaryQuote
{
    public sealed record AncillaryQuoteRequest(
        int CurrencyId,
        DateTimeOffset AsOf,
        IReadOnlyList<AncillaryQuoteTraveller> Travellers,
        IReadOnlyList<AncillaryQuoteBound> Bounds,
        IReadOnlyList<AncillaryQuoteExistingOccurrence> Existing,
        IReadOnlyList<AncillaryQuoteSelection> Selections);

    public sealed record AncillaryQuoteTraveller(
        string Ref,
        string PassengerTypeCode,
        IReadOnlyList<string> FlightRefs);

    public sealed record AncillaryQuoteBound(
        string Ref,
        IReadOnlyList<AncillaryQuoteFlight> Flights);

    public sealed record AncillaryQuoteFlight(
        string Ref,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset DepartureDateTime,
        int MarketingAirlineId);

    public sealed record AncillaryQuoteExistingOccurrence(
        string ProductRef,
        string TravellerRef,
        string? BoundRef,
        string? FlightRef,
        int Quantity);

    public sealed record AncillaryQuoteSelection(
        string ProductRef,
        int ProductVersion,
        long PriceRuleId,
        string TravellerRef,
        string? BoundRef,
        string? FlightRef,
        int Quantity);
}
