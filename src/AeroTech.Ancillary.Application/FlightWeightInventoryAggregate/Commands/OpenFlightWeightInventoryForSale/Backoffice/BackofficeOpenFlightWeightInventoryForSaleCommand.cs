using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.OpenFlightWeightInventoryForSale.Backoffice
{
    public sealed record BackofficeOpenFlightWeightInventoryForSaleCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<FlightWeightInventoryResult>, IOpenFlightWeightInventoryForSaleCommand;
}
