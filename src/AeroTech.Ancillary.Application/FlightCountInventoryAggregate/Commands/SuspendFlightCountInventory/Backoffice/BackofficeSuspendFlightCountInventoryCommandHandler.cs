using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.SuspendFlightCountInventory.Backoffice
{
    public sealed class BackofficeSuspendFlightCountInventoryCommandHandler
        : IRequestHandler<BackofficeSuspendFlightCountInventoryCommand, FlightCountInventoryResult>
    {
        private readonly ISuspendFlightCountInventoryService _service;

        public BackofficeSuspendFlightCountInventoryCommandHandler(ISuspendFlightCountInventoryService service) => _service = service;

        public Task<FlightCountInventoryResult> Handle(BackofficeSuspendFlightCountInventoryCommand command, CancellationToken cancellationToken)
            => _service.SuspendAsync(command, cancellationToken);
    }
}
