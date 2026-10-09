using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Projection
{
    internal static class FlightCountInventoryReadModelSnapshotFactory
    {
        public static FlightCountInventoryReadModelSnapshot ToReadModelSnapshot(this FlightCountInventory inventory)
            => new(
                inventory.Id,
                inventory.OwnerAirlineId,
                inventory.FlightId,
                inventory.ResourceId,
                inventory.CountUnit,
                inventory.TotalCapacity,
                inventory.ClosedForSale,
                inventory.Status,
                inventory.Version,
                inventory.CreatedAt,
                inventory.UpdatedAt,
                inventory.Adjustments
                    .Select(adjustment => new FlightCountAdjustmentReadModelSnapshot(
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
