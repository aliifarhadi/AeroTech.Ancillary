using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory.Backoffice
{
    public sealed class BackofficeDefineFlightWeightInventoryCommandHandler
        : IRequestHandler<BackofficeDefineFlightWeightInventoryCommand, FlightWeightInventoryResult>
    {
        private readonly IDefineFlightWeightInventoryService _service;

        public BackofficeDefineFlightWeightInventoryCommandHandler(IDefineFlightWeightInventoryService service) => _service = service;

        public Task<FlightWeightInventoryResult> Handle(BackofficeDefineFlightWeightInventoryCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
