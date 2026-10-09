using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory.Backoffice
{
    public sealed class BackofficeAdjustFlightCountInventoryCommandHandler
        : IRequestHandler<BackofficeAdjustFlightCountInventoryCommand, FlightCountAdjustmentResult>
    {
        private readonly IAdjustFlightCountInventoryService _service;

        public BackofficeAdjustFlightCountInventoryCommandHandler(IAdjustFlightCountInventoryService service) => _service = service;

        public Task<FlightCountAdjustmentResult> Handle(BackofficeAdjustFlightCountInventoryCommand command, CancellationToken cancellationToken)
            => _service.AdjustAsync(command, cancellationToken);
    }
}
