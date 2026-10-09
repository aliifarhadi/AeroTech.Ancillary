using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.AdjustFlightWeightInventory.Backoffice
{
    public sealed class BackofficeAdjustFlightWeightInventoryCommandHandler
        : IRequestHandler<BackofficeAdjustFlightWeightInventoryCommand, FlightWeightAdjustmentResult>
    {
        private readonly IAdjustFlightWeightInventoryService _service;

        public BackofficeAdjustFlightWeightInventoryCommandHandler(IAdjustFlightWeightInventoryService service) => _service = service;

        public Task<FlightWeightAdjustmentResult> Handle(BackofficeAdjustFlightWeightInventoryCommand command, CancellationToken cancellationToken)
            => _service.AdjustAsync(command, cancellationToken);
    }
}
