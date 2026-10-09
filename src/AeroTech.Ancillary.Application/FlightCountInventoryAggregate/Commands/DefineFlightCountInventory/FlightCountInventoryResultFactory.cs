using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Entities;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory
{
    internal static class FlightCountInventoryResultFactory
    {
        public static FlightCountInventoryResult ToResult(this FlightCountInventory inventory)
            => new(
                inventory.Id,
                inventory.OwnerAirlineId,
                inventory.FlightId,
                inventory.ResourceId,
                inventory.CountUnit,
                inventory.TotalCapacity,
                inventory.ClosedForSale,
                inventory.Status,
                inventory.Version);

        public static FlightCountAdjustmentResult ToResult(this FlightCountAdjustment adjustment)
            => new(adjustment.FlightCountInventoryId, adjustment.Id, adjustment.PreviousTotal, adjustment.NewTotal, adjustment.ResultingVersion);
    }
}
