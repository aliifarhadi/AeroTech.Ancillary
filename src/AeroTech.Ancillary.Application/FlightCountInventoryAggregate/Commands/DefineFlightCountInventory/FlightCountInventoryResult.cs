using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory
{
    public sealed record FlightCountInventoryResult(
        long Id,
        int OwnerAirlineId,
        long FlightId,
        long ResourceId,
        InventoryCountUnit CountUnit,
        int TotalCapacity,
        bool ClosedForSale,
        InventoryRecordStatus Status,
        long Version);

    public sealed record FlightCountAdjustmentResult(
        long InventoryId,
        long AdjustmentId,
        int PreviousTotal,
        int NewTotal,
        long Version);
}
