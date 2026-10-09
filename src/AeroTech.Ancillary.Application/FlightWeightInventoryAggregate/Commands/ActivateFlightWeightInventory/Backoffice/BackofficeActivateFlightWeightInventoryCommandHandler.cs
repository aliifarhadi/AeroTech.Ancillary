using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.ActivateFlightWeightInventory.Backoffice
{
    public sealed class BackofficeActivateFlightWeightInventoryCommandHandler
        : IRequestHandler<BackofficeActivateFlightWeightInventoryCommand, FlightWeightInventoryResult>
    {
        private readonly IActivateFlightWeightInventoryService _service;

        public BackofficeActivateFlightWeightInventoryCommandHandler(IActivateFlightWeightInventoryService service) => _service = service;

        public Task<FlightWeightInventoryResult> Handle(BackofficeActivateFlightWeightInventoryCommand command, CancellationToken cancellationToken)
            => _service.ActivateAsync(command, cancellationToken);
    }
}
