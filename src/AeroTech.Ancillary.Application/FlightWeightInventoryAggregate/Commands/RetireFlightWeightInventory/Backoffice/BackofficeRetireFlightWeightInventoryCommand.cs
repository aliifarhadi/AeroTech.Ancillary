using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.RetireFlightWeightInventory.Backoffice
{
    public sealed record BackofficeRetireFlightWeightInventoryCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<FlightWeightInventoryResult>, IRetireFlightWeightInventoryCommand;
}
