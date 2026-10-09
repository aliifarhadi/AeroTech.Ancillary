using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using MediatR;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoriesPaginated.Backoffice
{
    public sealed class BackofficeGetFlightCountInventoriesPaginatedQueryHandler : IRequestHandler<BackofficeGetFlightCountInventoriesPaginatedQuery, GridData<FlightCountInventoryPaginatedRowDto>>
    {
        private readonly IGetFlightCountInventoriesPaginatedService _service;

        public BackofficeGetFlightCountInventoriesPaginatedQueryHandler(IGetFlightCountInventoriesPaginatedService service) => _service = service;

        public Task<GridData<FlightCountInventoryPaginatedRowDto>> Handle(BackofficeGetFlightCountInventoriesPaginatedQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
