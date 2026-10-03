using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public sealed record ServiceReservationResult(
        long ReservationId,
        string IdempotencyKey,
        string Reference,
        DateTimeOffset ExpiresAt,
        IReadOnlyList<ServiceReservationUnitResult> Units);

    public sealed record ServiceReservationUnitResult(
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
