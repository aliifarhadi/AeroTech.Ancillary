using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.ActivateAirportSlotInventory.Backoffice
{
    public sealed class BackofficeActivateAirportSlotInventoryCommandHandler
        : IRequestHandler<BackofficeActivateAirportSlotInventoryCommand, AirportSlotInventoryResult>
    {
        private readonly IActivateAirportSlotInventoryService _service;

        public BackofficeActivateAirportSlotInventoryCommandHandler(IActivateAirportSlotInventoryService service) => _service = service;

        public Task<AirportSlotInventoryResult> Handle(BackofficeActivateAirportSlotInventoryCommand command, CancellationToken cancellationToken)
            => _service.ActivateAsync(command, cancellationToken);
    }
}
