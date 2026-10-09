using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.SuspendFlightWeightInventory.Backoffice
{
    public sealed class BackofficeSuspendFlightWeightInventoryCommandHandler
        : IRequestHandler<BackofficeSuspendFlightWeightInventoryCommand, FlightWeightInventoryResult>
    {
        private readonly ISuspendFlightWeightInventoryService _service;

        public BackofficeSuspendFlightWeightInventoryCommandHandler(ISuspendFlightWeightInventoryService service) => _service = service;

        public Task<FlightWeightInventoryResult> Handle(BackofficeSuspendFlightWeightInventoryCommand command, CancellationToken cancellationToken)
            => _service.SuspendAsync(command, cancellationToken);
    }
}
