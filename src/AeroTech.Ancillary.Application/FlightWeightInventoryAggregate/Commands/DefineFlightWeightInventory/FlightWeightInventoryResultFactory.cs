using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Entities;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory
{
    internal static class FlightWeightInventoryResultFactory
    {
        public static FlightWeightInventoryResult ToResult(this FlightWeightInventory inventory)
            => new(
                inventory.Id,
                inventory.OwnerAirlineId,
                inventory.FlightId,
                inventory.WeightResourceId,
                inventory.CapacityKg,
                inventory.ClosedForSale,
                inventory.Status,
                inventory.Version);

        public static FlightWeightAdjustmentResult ToResult(this FlightWeightAdjustment adjustment)
            => new(adjustment.FlightWeightInventoryId, adjustment.Id, adjustment.PreviousKg, adjustment.NewKg, adjustment.ResultingVersion);
    }
}
