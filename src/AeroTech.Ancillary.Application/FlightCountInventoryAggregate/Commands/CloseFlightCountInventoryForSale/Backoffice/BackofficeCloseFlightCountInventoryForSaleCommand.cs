using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.CloseFlightCountInventoryForSale.Backoffice
{
    public sealed record BackofficeCloseFlightCountInventoryForSaleCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<FlightCountInventoryResult>, ICloseFlightCountInventoryForSaleCommand;
}
