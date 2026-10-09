using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoriesPaginated.Backoffice
{
    public sealed class BackofficeGetFlightCountInventoriesPaginatedQuery
        : PaginationQuery, IRequest<GridData<FlightCountInventoryPaginatedRowDto>>, IGetFlightCountInventoriesPaginatedQuery
    {
        public long? FlightId { get; set; }

        public long? ResourceId { get; set; }

        public InventoryRecordStatus? Status { get; set; }

        public bool? ClosedForSale { get; set; }
    }
}
