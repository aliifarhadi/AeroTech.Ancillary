using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.RetireFlightCountInventory.Backoffice
{
    public sealed class BackofficeRetireFlightCountInventoryCommandHandler
        : IRequestHandler<BackofficeRetireFlightCountInventoryCommand, FlightCountInventoryResult>
    {
        private readonly IRetireFlightCountInventoryService _service;

        public BackofficeRetireFlightCountInventoryCommandHandler(IRetireFlightCountInventoryService service) => _service = service;

        public Task<FlightCountInventoryResult> Handle(BackofficeRetireFlightCountInventoryCommand command, CancellationToken cancellationToken)
            => _service.RetireAsync(command, cancellationToken);
    }
}
