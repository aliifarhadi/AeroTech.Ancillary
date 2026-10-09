using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoriesPaginated
{
    public interface IGetAirportSlotInventoriesPaginatedQuery
    {
        int? AirportId { get; }

        long? FacilityId { get; }

        DateTimeOffset? FromUtc { get; }

        DateTimeOffset? ToUtc { get; }

        InventoryRecordStatus? Status { get; }

        bool? ClosedForSale { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
