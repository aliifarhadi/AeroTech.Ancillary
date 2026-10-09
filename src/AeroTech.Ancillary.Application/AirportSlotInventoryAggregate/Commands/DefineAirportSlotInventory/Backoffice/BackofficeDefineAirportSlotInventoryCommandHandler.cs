using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory.Backoffice
{
    public sealed class BackofficeDefineAirportSlotInventoryCommandHandler
        : IRequestHandler<BackofficeDefineAirportSlotInventoryCommand, AirportSlotInventoryResult>
    {
        private readonly IDefineAirportSlotInventoryService _service;

        public BackofficeDefineAirportSlotInventoryCommandHandler(IDefineAirportSlotInventoryService service) => _service = service;

        public Task<AirportSlotInventoryResult> Handle(BackofficeDefineAirportSlotInventoryCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
