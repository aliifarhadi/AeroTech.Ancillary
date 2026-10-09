using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.RetireAirportSlotInventory.Backoffice
{
    public sealed class BackofficeRetireAirportSlotInventoryCommandHandler
        : IRequestHandler<BackofficeRetireAirportSlotInventoryCommand, AirportSlotInventoryResult>
    {
        private readonly IRetireAirportSlotInventoryService _service;

        public BackofficeRetireAirportSlotInventoryCommandHandler(IRetireAirportSlotInventoryService service) => _service = service;

        public Task<AirportSlotInventoryResult> Handle(BackofficeRetireAirportSlotInventoryCommand command, CancellationToken cancellationToken)
            => _service.RetireAsync(command, cancellationToken);
    }
}
