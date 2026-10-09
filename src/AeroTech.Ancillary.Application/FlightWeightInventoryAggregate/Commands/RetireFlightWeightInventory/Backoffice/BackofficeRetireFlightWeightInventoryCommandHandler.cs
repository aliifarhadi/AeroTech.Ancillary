using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.RetireFlightWeightInventory.Backoffice
{
    public sealed class BackofficeRetireFlightWeightInventoryCommandHandler
        : IRequestHandler<BackofficeRetireFlightWeightInventoryCommand, FlightWeightInventoryResult>
    {
        private readonly IRetireFlightWeightInventoryService _service;

        public BackofficeRetireFlightWeightInventoryCommandHandler(IRetireFlightWeightInventoryService service) => _service = service;

        public Task<FlightWeightInventoryResult> Handle(BackofficeRetireFlightWeightInventoryCommand command, CancellationToken cancellationToken)
            => _service.RetireAsync(command, cancellationToken);
    }
}
