using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments
{
    public sealed record AncillaryReservationAllocationArgs(
        InventoryResourceKind ResourceKind,
        long InventorySourceId,
        decimal Amount);
}
