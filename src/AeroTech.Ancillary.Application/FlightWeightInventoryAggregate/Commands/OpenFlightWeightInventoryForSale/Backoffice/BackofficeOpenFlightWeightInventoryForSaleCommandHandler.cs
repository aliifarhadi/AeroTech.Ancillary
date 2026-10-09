using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.OpenFlightWeightInventoryForSale.Backoffice
{
    public sealed class BackofficeOpenFlightWeightInventoryForSaleCommandHandler
        : IRequestHandler<BackofficeOpenFlightWeightInventoryForSaleCommand, FlightWeightInventoryResult>
    {
        private readonly IOpenFlightWeightInventoryForSaleService _service;

        public BackofficeOpenFlightWeightInventoryForSaleCommandHandler(IOpenFlightWeightInventoryForSaleService service) => _service = service;

        public Task<FlightWeightInventoryResult> Handle(BackofficeOpenFlightWeightInventoryForSaleCommand command, CancellationToken cancellationToken)
            => _service.OpenAsync(command, cancellationToken);
    }
}
