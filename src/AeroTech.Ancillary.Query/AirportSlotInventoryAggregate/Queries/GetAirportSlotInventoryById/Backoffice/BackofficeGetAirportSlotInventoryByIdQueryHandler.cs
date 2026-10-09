using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoryById.Backoffice
{
    public sealed class BackofficeGetAirportSlotInventoryByIdQueryHandler : IRequestHandler<BackofficeGetAirportSlotInventoryByIdQuery, BackofficeAirportSlotInventoryDto>
    {
        private readonly IGetAirportSlotInventoryByIdService _service;

        public BackofficeGetAirportSlotInventoryByIdQueryHandler(IGetAirportSlotInventoryByIdService service) => _service = service;

        public Task<BackofficeAirportSlotInventoryDto> Handle(BackofficeGetAirportSlotInventoryByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.InventoryId, cancellationToken);
    }
}
