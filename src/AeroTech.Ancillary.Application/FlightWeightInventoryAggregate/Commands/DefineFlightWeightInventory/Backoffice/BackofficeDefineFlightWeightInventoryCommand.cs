using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory.Backoffice
{
    public sealed record BackofficeDefineFlightWeightInventoryCommand(
        int OwnerAirlineId,
        long FlightId,
        long WeightResourceId,
        decimal CapacityKg) : IRequest<FlightWeightInventoryResult>, IDefineFlightWeightInventoryCommand;
}
