using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using MediatR;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoriesPaginated.Backoffice
{
    public sealed class BackofficeGetAirportSlotInventoriesPaginatedQueryHandler : IRequestHandler<BackofficeGetAirportSlotInventoriesPaginatedQuery, GridData<AirportSlotInventoryPaginatedRowDto>>
    {
        private readonly IGetAirportSlotInventoriesPaginatedService _service;

        public BackofficeGetAirportSlotInventoriesPaginatedQueryHandler(IGetAirportSlotInventoriesPaginatedService service) => _service = service;

        public Task<GridData<AirportSlotInventoryPaginatedRowDto>> Handle(BackofficeGetAirportSlotInventoriesPaginatedQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
