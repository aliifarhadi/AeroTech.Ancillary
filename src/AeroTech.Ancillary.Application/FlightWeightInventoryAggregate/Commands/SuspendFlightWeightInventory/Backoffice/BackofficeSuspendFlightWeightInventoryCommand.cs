using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.SuspendFlightWeightInventory.Backoffice
{
    public sealed record BackofficeSuspendFlightWeightInventoryCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<FlightWeightInventoryResult>, ISuspendFlightWeightInventoryCommand;
}
