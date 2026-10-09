using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoriesPaginated
{
    public interface IGetFlightWeightInventoriesPaginatedQuery
    {
        long? FlightId { get; }

        long? WeightResourceId { get; }

        InventoryRecordStatus? Status { get; }

        bool? ClosedForSale { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
