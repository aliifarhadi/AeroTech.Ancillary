using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.OpenFlightCountInventoryForSale.Backoffice
{
    public sealed class BackofficeOpenFlightCountInventoryForSaleCommandHandler
        : IRequestHandler<BackofficeOpenFlightCountInventoryForSaleCommand, FlightCountInventoryResult>
    {
        private readonly IOpenFlightCountInventoryForSaleService _service;

        public BackofficeOpenFlightCountInventoryForSaleCommandHandler(IOpenFlightCountInventoryForSaleService service) => _service = service;

        public Task<FlightCountInventoryResult> Handle(BackofficeOpenFlightCountInventoryForSaleCommand command, CancellationToken cancellationToken)
            => _service.OpenAsync(command, cancellationToken);
    }
}
