using System.Globalization;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.AncillaryQuote;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public static class Quotes
{
    public static readonly DateTimeOffset AsOf = Instant("2026-10-01T14:00:00+00:00");

    public static DateTimeOffset Instant(string value) => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);

    public static AncillaryQuoteFlight Flight(
        string reference,
        int originAirportId = 1,
        int destinationAirportId = 2,
        string departure = "2026-10-10T08:00:00+03:30",
        int marketingAirlineId = SubCodes.AirlineId)
        => new(reference, originAirportId, destinationAirportId, Instant(departure), marketingAirlineId);

    public static AncillaryQuoteBound Bound(string reference, params AncillaryQuoteFlight[] flights) => new(reference, flights);

    public static AncillaryQuoteTraveller Traveller(string reference, string passengerTypeCode, params string[] flightRefs)
        => new(reference, passengerTypeCode, flightRefs);

    public static AncillaryQuoteRequest Request(
        IReadOnlyList<AncillaryQuoteTraveller> travellers,
        IReadOnlyList<AncillaryQuoteBound> bounds,
        IReadOnlyList<AncillaryQuoteSelection>? selections = null,
        IReadOnlyList<AncillaryQuoteExistingOccurrence>? existing = null,
        int currencyId = RuleSpec.Currency,
        DateTimeOffset? asOf = null)
        => new(currencyId, asOf ?? AsOf, travellers, bounds, existing ?? [], selections ?? []);

    public static AncillaryQuoteRequest Golden(params AncillaryQuoteSelection[] selections)
        => Request([Traveller("T1", "ADT", "F1")], [Bound("B1", Flight("F1"))], selections);

    public static AncillaryQuoteSelection Selection(
        AncillaryProduct product,
        AncillaryPriceRule rule,
        string travellerRef = "T1",
        string? boundRef = "B1",
        int quantity = 1,
        string? flightRef = null)
        => new(product.ProductRef, product.Version, rule.Id, travellerRef, boundRef, flightRef, quantity);

    public static AncillaryQuoteExistingOccurrence Existing(string productRef, string travellerRef, string? boundRef, int quantity)
        => new(productRef, travellerRef, boundRef, null, quantity);

    public static AncillaryQuoteExistingOccurrence ExistingOnFlight(string productRef, string travellerRef, string flightRef, int quantity)
        => new(productRef, travellerRef, null, flightRef, quantity);

    public static AncillaryQuoteBound[] TwoOneFlightBounds(int secondOriginAirportId = 2)
        =>
        [
            Bound("B1", Flight("F1")),
            Bound("B2", Flight("F2", secondOriginAirportId, 1, "2026-10-15T08:00:00+03:30"))
        ];

    public static AncillaryQuoteRequest LoungeGolden(params AncillaryQuoteSelection[] selections)
        => Request([Traveller("T1", "ADT", "F1", "F2")], TwoOneFlightBounds(), selections);

    public static AncillaryQuoteSelection LoungeSelection(
        AncillaryProduct product,
        AncillaryPriceRule rule,
        string travellerRef = "T1",
        string flightRef = "F1",
        string? boundRef = null,
        int quantity = 1)
        => new(product.ProductRef, product.Version, rule.Id, travellerRef, boundRef, flightRef, quantity);

    public static (string ProductRef, string TravellerRef, string BoundRef, string? FlightRef) Position(AncillaryQuoteItem item)
        => (item.Product.ProductRef, item.TravellerRef, item.BoundRef, item.FlightRef);

    public static (string ProductRef, string TravellerRef, string BoundRef) Occurrence(AncillaryQuoteItem item)
        => (item.Product.ProductRef, item.TravellerRef, item.BoundRef);
}
