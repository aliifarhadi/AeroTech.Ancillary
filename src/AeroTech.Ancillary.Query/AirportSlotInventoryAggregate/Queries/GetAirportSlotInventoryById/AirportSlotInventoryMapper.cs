using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Dto;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;
using System.Globalization;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoryById
{
    public static class AirportSlotInventoryMapper
    {
        public static BackofficeAirportSlotInventoryDto ToBackofficeInventory(AirportSlotInventoryReadModel inventory, IEnumerable<AirportSlotAdjustmentReadModel> adjustments)
            => new(
                inventory.Id,
                inventory.OwnerAirlineId,
                inventory.AirportId,
                inventory.FacilityId,
                inventory.StartUtc,
                inventory.EndUtc,
                inventory.CapacityPersons,
                inventory.ClosedForSale,
                EnumValueDto.Of(inventory.Status),
                inventory.Version,
                inventory.CreatedAt,
                inventory.UpdatedAt,
                adjustments
                    .OrderBy(adjustment => adjustment.ResultingVersion)
                    .Select(adjustment => new BackofficeAirportSlotAdjustmentDto(
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

        public static AirportSlotInventoryPaginatedRowDto ToPaginatedRow(AirportSlotInventoryReadModel inventory)
            => new()
            {
                Id = inventory.Id.ToString(CultureInfo.InvariantCulture),
                OwnerAirlineId = inventory.OwnerAirlineId,
                AirportId = inventory.AirportId,
                FacilityId = inventory.FacilityId.ToString(CultureInfo.InvariantCulture),
                StartUtc = inventory.StartUtc,
                EndUtc = inventory.EndUtc,
                CapacityPersons = inventory.CapacityPersons,
                ClosedForSale = inventory.ClosedForSale,
                Status = EnumValueDto.Of(inventory.Status),
                Version = inventory.Version,
                AdjustmentCount = inventory.AdjustmentCount,
                UpdatedAt = inventory.UpdatedAt
            };
    }
}
