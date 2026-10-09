using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.CloseFlightCountInventoryForSale.Backoffice
{
    public sealed class BackofficeCloseFlightCountInventoryForSaleCommandHandler
        : IRequestHandler<BackofficeCloseFlightCountInventoryForSaleCommand, FlightCountInventoryResult>
    {
        private readonly ICloseFlightCountInventoryForSaleService _service;

        public BackofficeCloseFlightCountInventoryForSaleCommandHandler(ICloseFlightCountInventoryForSaleService service) => _service = service;

        public Task<FlightCountInventoryResult> Handle(BackofficeCloseFlightCountInventoryForSaleCommand command, CancellationToken cancellationToken)
            => _service.CloseAsync(command, cancellationToken);
    }
}
