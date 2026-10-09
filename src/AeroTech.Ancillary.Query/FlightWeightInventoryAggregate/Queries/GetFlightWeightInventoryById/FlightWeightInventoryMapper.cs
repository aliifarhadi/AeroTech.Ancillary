using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Dto;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;
using System.Globalization;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoryById
{
    public static class FlightWeightInventoryMapper
    {
        public static BackofficeFlightWeightInventoryDto ToBackofficeInventory(FlightWeightInventoryReadModel inventory, IEnumerable<FlightWeightAdjustmentReadModel> adjustments)
            => new(
                inventory.Id,
                inventory.OwnerAirlineId,
                inventory.FlightId,
                inventory.WeightResourceId,
                inventory.CapacityKg,
                inventory.ClosedForSale,
                EnumValueDto.Of(inventory.Status),
                inventory.Version,
                inventory.CreatedAt,
                inventory.UpdatedAt,
                adjustments
                    .OrderBy(adjustment => adjustment.ResultingVersion)
                    .Select(adjustment => new BackofficeFlightWeightAdjustmentDto(
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

        public static FlightWeightInventoryPaginatedRowDto ToPaginatedRow(FlightWeightInventoryReadModel inventory)
            => new()
            {
                Id = inventory.Id.ToString(CultureInfo.InvariantCulture),
                OwnerAirlineId = inventory.OwnerAirlineId,
                FlightId = inventory.FlightId.ToString(CultureInfo.InvariantCulture),
                WeightResourceId = inventory.WeightResourceId.ToString(CultureInfo.InvariantCulture),
                CapacityKg = inventory.CapacityKg,
                ClosedForSale = inventory.ClosedForSale,
                Status = EnumValueDto.Of(inventory.Status),
                Version = inventory.Version,
                AdjustmentCount = inventory.AdjustmentCount,
                UpdatedAt = inventory.UpdatedAt
            };
    }
}
