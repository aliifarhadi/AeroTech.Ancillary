using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ServiceReservationAggregate.Arguments
{
    public sealed record ServiceReservationUnitArgs(
        string UnitReference,
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
