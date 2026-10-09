using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.ActivateFlightCountInventory.Backoffice
{
    public sealed class BackofficeActivateFlightCountInventoryCommandHandler
        : IRequestHandler<BackofficeActivateFlightCountInventoryCommand, FlightCountInventoryResult>
    {
        private readonly IActivateFlightCountInventoryService _service;

        public BackofficeActivateFlightCountInventoryCommandHandler(IActivateFlightCountInventoryService service) => _service = service;

        public Task<FlightCountInventoryResult> Handle(BackofficeActivateFlightCountInventoryCommand command, CancellationToken cancellationToken)
            => _service.ActivateAsync(command, cancellationToken);
    }
}
