using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory.Backoffice
{
    public sealed class BackofficeDefineFlightCountInventoryCommandHandler
        : IRequestHandler<BackofficeDefineFlightCountInventoryCommand, FlightCountInventoryResult>
    {
        private readonly IDefineFlightCountInventoryService _service;

        public BackofficeDefineFlightCountInventoryCommandHandler(IDefineFlightCountInventoryService service) => _service = service;

        public Task<FlightCountInventoryResult> Handle(BackofficeDefineFlightCountInventoryCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
