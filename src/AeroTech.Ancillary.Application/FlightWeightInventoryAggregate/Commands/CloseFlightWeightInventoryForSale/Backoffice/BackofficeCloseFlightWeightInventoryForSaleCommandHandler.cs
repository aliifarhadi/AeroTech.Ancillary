using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.CloseFlightWeightInventoryForSale.Backoffice
{
    public sealed class BackofficeCloseFlightWeightInventoryForSaleCommandHandler
        : IRequestHandler<BackofficeCloseFlightWeightInventoryForSaleCommand, FlightWeightInventoryResult>
    {
        private readonly ICloseFlightWeightInventoryForSaleService _service;

        public BackofficeCloseFlightWeightInventoryForSaleCommandHandler(ICloseFlightWeightInventoryForSaleService service) => _service = service;

        public Task<FlightWeightInventoryResult> Handle(BackofficeCloseFlightWeightInventoryForSaleCommand command, CancellationToken cancellationToken)
            => _service.CloseAsync(command, cancellationToken);
    }
}
