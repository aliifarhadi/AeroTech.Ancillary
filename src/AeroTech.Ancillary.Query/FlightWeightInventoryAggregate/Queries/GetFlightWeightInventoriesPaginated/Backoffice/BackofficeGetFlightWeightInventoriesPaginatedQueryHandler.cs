using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using MediatR;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoriesPaginated.Backoffice
{
    public sealed class BackofficeGetFlightWeightInventoriesPaginatedQueryHandler : IRequestHandler<BackofficeGetFlightWeightInventoriesPaginatedQuery, GridData<FlightWeightInventoryPaginatedRowDto>>
    {
        private readonly IGetFlightWeightInventoriesPaginatedService _service;

        public BackofficeGetFlightWeightInventoriesPaginatedQueryHandler(IGetFlightWeightInventoriesPaginatedService service) => _service = service;

        public Task<GridData<FlightWeightInventoryPaginatedRowDto>> Handle(BackofficeGetFlightWeightInventoriesPaginatedQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
