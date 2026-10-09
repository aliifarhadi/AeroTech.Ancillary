using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.ActivateFlightWeightInventory.Backoffice
{
    public sealed record BackofficeActivateFlightWeightInventoryCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<FlightWeightInventoryResult>, IActivateFlightWeightInventoryCommand;
}
