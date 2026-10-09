using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.AdjustAirportSlotInventory.Backoffice
{
    public sealed class BackofficeAdjustAirportSlotInventoryCommandHandler
        : IRequestHandler<BackofficeAdjustAirportSlotInventoryCommand, AirportSlotAdjustmentResult>
    {
        private readonly IAdjustAirportSlotInventoryService _service;

        public BackofficeAdjustAirportSlotInventoryCommandHandler(IAdjustAirportSlotInventoryService service) => _service = service;

        public Task<AirportSlotAdjustmentResult> Handle(BackofficeAdjustAirportSlotInventoryCommand command, CancellationToken cancellationToken)
            => _service.AdjustAsync(command, cancellationToken);
    }
}
