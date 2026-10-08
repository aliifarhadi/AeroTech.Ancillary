using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Oracle;

public enum OracleVerdict
{
    Match = 1,
    NoMatch = 2,
    UnsupportedContext = 3
}

public enum OracleOutcome
{
    Paid = 1,
    Free = 2,
    NotAvailable = 3,
    NoMatch = 4,
    UnsupportedContext = 5
}

public sealed record OracleDecision(OracleOutcome Outcome, long? ProvisionId);

public sealed record OracleOccurrence
{
    private OracleOccurrence(DateTimeOffset? instant, DateTime? localTime, TimeZoneInfo? zone)
    {
        Instant = instant;
        LocalTime = localTime;
        Zone = zone;
    }

    public DateTimeOffset? Instant { get; }

    public DateTime? LocalTime { get; }

    public TimeZoneInfo? Zone { get; }

    public static OracleOccurrence AtInstant(DateTimeOffset instant, TimeZoneInfo? zone) => new(instant, null, zone);

    public static OracleOccurrence AtLocal(DateTime localTime, TimeZoneInfo? zone) => new(null, DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified), zone);

    public bool TryResolve(out DateTime local, out DateTimeOffset instant)
    {
        local = default;
        instant = default;

        if (Zone is null)
            return false;

        if (Instant is not null)
        {
            instant = Instant.Value;
            local = TimeZoneInfo.ConvertTime(instant, Zone).DateTime;

            return true;
        }

        if (LocalTime is null || Zone.IsAmbiguousTime(LocalTime.Value) || Zone.IsInvalidTime(LocalTime.Value))
            return false;

        local = LocalTime.Value;
        instant = new DateTimeOffset(local, Zone.GetUtcOffset(local));

        return true;
    }
}

public sealed record OracleServicePlace(ServiceLocationType LocationType, int LocationId);

public sealed record OracleContext
{
    public DateTimeOffset? SaleInstant { get; init; }

    public DateTimeOffset? TicketedAt { get; init; }

    public OracleOccurrence? Occurrence { get; init; }

    public PassengerTypeCode? PassengerType { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    public long? PointOfSaleId { get; init; }

    public long? CustomerId { get; init; }

    public CustomerType? CustomerType { get; init; }

    public int? OriginAirportId { get; init; }

    public int? DestinationAirportId { get; init; }

    public IReadOnlyList<int>? ViaAirportIds { get; init; }

    public IReadOnlyList<OracleServicePlace>? ServicePlaces { get; init; }

    public int? CoverageCountryId { get; init; }

    public int? MarketingAirlineId { get; init; }

    public int? OperatingAirlineId { get; init; }

    public string? FlightNumber { get; init; }

    public long? FlightId { get; init; }

    public int? AircraftId { get; init; }

    public long? AirFareId { get; init; }

    public AirFareType? AirFareType { get; init; }

    public long? FareFamilyId { get; init; }

    public string? FareBasisCode { get; init; }

    public int? CabinClassId { get; init; }

    public long? RbdId { get; init; }

    public string? SeatNumber { get; init; }

    public IReadOnlyList<string>? SeatCharacteristicCodes { get; init; }
}
