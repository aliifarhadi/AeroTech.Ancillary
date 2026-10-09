using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.OpenAirportSlotInventoryForSale.Backoffice
{
    public sealed class BackofficeOpenAirportSlotInventoryForSaleCommandHandler
        : IRequestHandler<BackofficeOpenAirportSlotInventoryForSaleCommand, AirportSlotInventoryResult>
    {
        private readonly IOpenAirportSlotInventoryForSaleService _service;

        public BackofficeOpenAirportSlotInventoryForSaleCommandHandler(IOpenAirportSlotInventoryForSaleService service) => _service = service;

        public Task<AirportSlotInventoryResult> Handle(BackofficeOpenAirportSlotInventoryForSaleCommand command, CancellationToken cancellationToken)
            => _service.OpenAsync(command, cancellationToken);
    }
}
