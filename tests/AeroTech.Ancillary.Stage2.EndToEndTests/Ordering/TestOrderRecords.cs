using System.Text.Json.Nodes;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

public sealed record TestOrderTraveller(string TravellerRef, long OrderTravellerId, string PassengerTypeCode, DateOnly? DateOfBirth);

public sealed record TestOrderAirService(
    long OrderServiceId,
    long OrderTravellerId,
    string TravellerRef,
    string BoundId,
    long FlightId,
    long FlightCapacityId,
    string FulfillmentProviderKey);

public sealed record TestOrderAncillaryService(
    long OrderServiceId,
    long OrderTravellerId,
    string TravellerRef,
    string Kind,
    string ServiceDefinitionRef,
    string VariantCode,
    string? Rfisc,
    string? SsrCode,
    string CoverageScope,
    IReadOnlyList<long> FlightIds,
    IReadOnlyList<string> PortionRefs,
    string? SeatNumber,
    long? FlightCapacityId,
    int? Quantity,
    int CurrencyId,
    decimal LineTotal,
    string FulfillmentProviderKey,
    JsonNode? Selection,
    int CreatedAtVersion);

public sealed record TestOrderSeat(
    long OrderSeatId,
    long OrderTravellerId,
    string TravellerRef,
    long FlightId,
    long FlightCapacityId,
    string SeatNumber,
    int CurrencyId,
    decimal Fee,
    string FulfillmentProviderKey,
    int CreatedAtVersion);

public sealed record TestAddRecord(string PayloadDigest, int Status, JsonObject Result);
