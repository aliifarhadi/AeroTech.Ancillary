using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Projection
{
    internal static class FlightWeightInventoryReadModelSnapshotFactory
    {
        public static FlightWeightInventoryReadModelSnapshot ToReadModelSnapshot(this FlightWeightInventory inventory)
            => new(
                inventory.Id,
                inventory.OwnerAirlineId,
                inventory.FlightId,
                inventory.WeightResourceId,
                inventory.CapacityKg,
                inventory.ClosedForSale,
                inventory.Status,
                inventory.Version,
                inventory.CreatedAt,
                inventory.UpdatedAt,
                inventory.Adjustments
                    .Select(adjustment => new FlightWeightAdjustmentReadModelSnapshot(
                        adjustment.Id,
                        adjustment.PreviousKg,
                        adjustment.NewKg,
                        adjustment.ReasonCode,
                        adjustment.ActorId,
                        adjustment.CorrelationId,
                        adjustment.OccurredAt,
                        adjustment.ExpectedVersion,
                        adjustment.ResultingVersion))
                    .ToList());
    }
}
