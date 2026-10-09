using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Dto;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;
using System.Globalization;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoryById
{
    public static class FlightCountInventoryMapper
    {
        public static BackofficeFlightCountInventoryDto ToBackofficeInventory(FlightCountInventoryReadModel inventory, IEnumerable<FlightCountAdjustmentReadModel> adjustments)
            => new(
                inventory.Id,
                inventory.OwnerAirlineId,
                inventory.FlightId,
                inventory.ResourceId,
                EnumValueDto.Of(inventory.CountUnit),
                inventory.TotalCapacity,
                inventory.ClosedForSale,
                EnumValueDto.Of(inventory.Status),
                inventory.Version,
                inventory.CreatedAt,
                inventory.UpdatedAt,
                adjustments
                    .OrderBy(adjustment => adjustment.ResultingVersion)
                    .Select(adjustment => new BackofficeFlightCountAdjustmentDto(
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

        public static FlightCountInventoryPaginatedRowDto ToPaginatedRow(FlightCountInventoryReadModel inventory)
            => new()
            {
                Id = inventory.Id.ToString(CultureInfo.InvariantCulture),
                OwnerAirlineId = inventory.OwnerAirlineId,
                FlightId = inventory.FlightId.ToString(CultureInfo.InvariantCulture),
                ResourceId = inventory.ResourceId.ToString(CultureInfo.InvariantCulture),
                CountUnit = EnumValueDto.Of(inventory.CountUnit),
                TotalCapacity = inventory.TotalCapacity,
                ClosedForSale = inventory.ClosedForSale,
                Status = EnumValueDto.Of(inventory.Status),
                Version = inventory.Version,
                AdjustmentCount = inventory.AdjustmentCount,
                UpdatedAt = inventory.UpdatedAt
            };
    }
}
