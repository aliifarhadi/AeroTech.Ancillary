using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.ServiceReservationAggregate.Dto
{
    public sealed record ServiceReservationDto(
        long ReservationId,
        string IdempotencyKey,
        string Reference,
        DateTimeOffset ExpiresAt,
        IReadOnlyList<ServiceReservationUnitDto> Units);

    public sealed record ServiceReservationUnitDto(
        long UnitRef,
        string UnitReference,
        ServiceReservationUnitStatus Status,
        int OwnerAirlineId,
        string ProductRef,
        int ProductVersion,
        long PriceRuleId,
        string TravellerRef,
        string BoundRef,
        string? FlightRef,
        IReadOnlyList<long> CoveredFlightIds,
        int Quantity,
        int CurrencyId,
        decimal Total,
        AncillaryInventoryControl InventoryControl);
}
