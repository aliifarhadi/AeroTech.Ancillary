using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoriesPaginated
{
    public interface IGetFlightCountInventoriesPaginatedQuery
    {
        long? FlightId { get; }

        long? ResourceId { get; }

        InventoryRecordStatus? Status { get; }

        bool? ClosedForSale { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
