using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.SuspendAirportSlotInventory.Backoffice
{
    public sealed class BackofficeSuspendAirportSlotInventoryCommandHandler
        : IRequestHandler<BackofficeSuspendAirportSlotInventoryCommand, AirportSlotInventoryResult>
    {
        private readonly ISuspendAirportSlotInventoryService _service;

        public BackofficeSuspendAirportSlotInventoryCommandHandler(ISuspendAirportSlotInventoryService service) => _service = service;

        public Task<AirportSlotInventoryResult> Handle(BackofficeSuspendAirportSlotInventoryCommand command, CancellationToken cancellationToken)
            => _service.SuspendAsync(command, cancellationToken);
    }
}
