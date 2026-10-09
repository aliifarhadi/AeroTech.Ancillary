using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Projection
{
    internal static class AirportSlotInventoryReadModelSnapshotFactory
    {
        public static AirportSlotInventoryReadModelSnapshot ToReadModelSnapshot(this AirportSlotInventory inventory)
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
                inventory.Version,
                inventory.CreatedAt,
                inventory.UpdatedAt,
                inventory.Adjustments
                    .Select(adjustment => new AirportSlotAdjustmentReadModelSnapshot(
                        adjustment.Id,
                        adjustment.PreviousTotal,
                        adjustment.NewTotal,
                        adjustment.ReasonCode,
                        adjustment.ActorId,
                        adjustment.CorrelationId,
                        adjustment.OccurredAt,
                        adjustment.ExpectedVersion,
                        adjustment.ResultingVersion))
                    .ToList());
    }
}
