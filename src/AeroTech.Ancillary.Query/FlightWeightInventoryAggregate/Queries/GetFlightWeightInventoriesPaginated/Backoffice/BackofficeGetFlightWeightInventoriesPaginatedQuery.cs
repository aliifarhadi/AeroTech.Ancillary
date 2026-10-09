using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoriesPaginated.Backoffice
{
    public sealed class BackofficeGetFlightWeightInventoriesPaginatedQuery
        : PaginationQuery, IRequest<GridData<FlightWeightInventoryPaginatedRowDto>>, IGetFlightWeightInventoriesPaginatedQuery
    {
        public long? FlightId { get; set; }

        public long? WeightResourceId { get; set; }

        public InventoryRecordStatus? Status { get; set; }

        public bool? ClosedForSale { get; set; }
    }
}
