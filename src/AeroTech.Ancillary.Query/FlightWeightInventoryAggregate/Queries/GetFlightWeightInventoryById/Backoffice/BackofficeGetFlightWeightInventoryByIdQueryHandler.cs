using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoryById.Backoffice
{
    public sealed class BackofficeGetFlightWeightInventoryByIdQueryHandler : IRequestHandler<BackofficeGetFlightWeightInventoryByIdQuery, BackofficeFlightWeightInventoryDto>
    {
        private readonly IGetFlightWeightInventoryByIdService _service;

        public BackofficeGetFlightWeightInventoryByIdQueryHandler(IGetFlightWeightInventoryByIdService service) => _service = service;

        public Task<BackofficeFlightWeightInventoryDto> Handle(BackofficeGetFlightWeightInventoryByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.InventoryId, cancellationToken);
    }
}
