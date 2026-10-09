using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory
{
    public sealed record AirportSlotInventoryResult(
        long Id,
        int OwnerAirlineId,
        int AirportId,
        long FacilityId,
        DateTimeOffset StartUtc,
        DateTimeOffset EndUtc,
        int CapacityPersons,
        bool ClosedForSale,
        InventoryRecordStatus Status,
        long Version);

    public sealed record AirportSlotAdjustmentResult(
        long InventoryId,
        long AdjustmentId,
        int PreviousTotal,
        int NewTotal,
        long Version);
}
