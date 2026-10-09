using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.CloseFlightWeightInventoryForSale.Backoffice
{
    public sealed record BackofficeCloseFlightWeightInventoryForSaleCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<FlightWeightInventoryResult>, ICloseFlightWeightInventoryForSaleCommand;
}
