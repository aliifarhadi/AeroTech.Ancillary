using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoriesPaginated.Backoffice
{
    public sealed class BackofficeGetAirportSlotInventoriesPaginatedQuery
        : PaginationQuery, IRequest<GridData<AirportSlotInventoryPaginatedRowDto>>, IGetAirportSlotInventoriesPaginatedQuery
    {
        public int? AirportId { get; set; }

        public long? FacilityId { get; set; }

        public DateTimeOffset? FromUtc { get; set; }

        public DateTimeOffset? ToUtc { get; set; }

        public InventoryRecordStatus? Status { get; set; }

        public bool? ClosedForSale { get; set; }
    }
}
