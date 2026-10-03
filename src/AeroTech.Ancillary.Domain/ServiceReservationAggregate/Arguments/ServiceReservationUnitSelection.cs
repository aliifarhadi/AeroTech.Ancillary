namespace AeroTech.Ancillary.Domain.ServiceReservationAggregate.Arguments
{
    public sealed record ServiceReservationUnitSelection(
        string UnitReference,
        string ProductRef,
        int ProductVersion,
        long PriceRuleId,
        string TravellerRef,
        string? BoundRef,
        string? FlightRef,
        int Quantity);
}
