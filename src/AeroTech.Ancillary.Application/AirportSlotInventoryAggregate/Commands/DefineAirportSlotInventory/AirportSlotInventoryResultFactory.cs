using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Entities;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory
{
    internal static class AirportSlotInventoryResultFactory
    {
        public static AirportSlotInventoryResult ToResult(this AirportSlotInventory inventory)
            => new(
                inventory.Id,
                inventory.OwnerAirlineId,
                inventory.AirportId,
                inventory.FacilityId,
                inventory.StartUtc,
                inventory.EndUtc,
                inventory.CapacityPersons,
                inventory.ClosedForSale,
                inventory.Status,
                inventory.Version);

        public static AirportSlotAdjustmentResult ToResult(this AirportSlotAdjustment adjustment)
            => new(adjustment.AirportSlotInventoryId, adjustment.Id, adjustment.PreviousTotal, adjustment.NewTotal, adjustment.ResultingVersion);
    }
}
