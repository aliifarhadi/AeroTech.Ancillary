using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.OpenFlightCountInventoryForSale.Backoffice
{
    public sealed record BackofficeOpenFlightCountInventoryForSaleCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<FlightCountInventoryResult>, IOpenFlightCountInventoryForSaleCommand;
}
