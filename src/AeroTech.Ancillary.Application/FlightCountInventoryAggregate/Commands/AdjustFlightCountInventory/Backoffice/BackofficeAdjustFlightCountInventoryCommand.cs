using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory.Backoffice
{
    public sealed record BackofficeAdjustFlightCountInventoryCommand(
        long InventoryId,
        int NewTotal,
        string ReasonCode,
        string CorrelationId,
        long ExpectedVersion) : IRequest<FlightCountAdjustmentResult>, IAdjustFlightCountInventoryCommand;
}
