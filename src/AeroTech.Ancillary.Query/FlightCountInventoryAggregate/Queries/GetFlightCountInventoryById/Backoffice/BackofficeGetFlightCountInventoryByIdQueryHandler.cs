using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoryById.Backoffice
{
    public sealed class BackofficeGetFlightCountInventoryByIdQueryHandler : IRequestHandler<BackofficeGetFlightCountInventoryByIdQuery, BackofficeFlightCountInventoryDto>
    {
        private readonly IGetFlightCountInventoryByIdService _service;

        public BackofficeGetFlightCountInventoryByIdQueryHandler(IGetFlightCountInventoryByIdService service) => _service = service;

        public Task<BackofficeFlightCountInventoryDto> Handle(BackofficeGetFlightCountInventoryByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.InventoryId, cancellationToken);
    }
}
