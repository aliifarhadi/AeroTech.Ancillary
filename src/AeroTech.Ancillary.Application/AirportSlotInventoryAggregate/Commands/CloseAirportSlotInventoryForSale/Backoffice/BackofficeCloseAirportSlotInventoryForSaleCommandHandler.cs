using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.CloseAirportSlotInventoryForSale.Backoffice
{
    public sealed class BackofficeCloseAirportSlotInventoryForSaleCommandHandler
        : IRequestHandler<BackofficeCloseAirportSlotInventoryForSaleCommand, AirportSlotInventoryResult>
    {
        private readonly ICloseAirportSlotInventoryForSaleService _service;

        public BackofficeCloseAirportSlotInventoryForSaleCommandHandler(ICloseAirportSlotInventoryForSaleService service) => _service = service;

        public Task<AirportSlotInventoryResult> Handle(BackofficeCloseAirportSlotInventoryForSaleCommand command, CancellationToken cancellationToken)
            => _service.CloseAsync(command, cancellationToken);
    }
}
