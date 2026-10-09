namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory
{
    public interface IDefineFlightWeightInventoryCommand
    {
        int OwnerAirlineId { get; }

        long FlightId { get; }

        long WeightResourceId { get; }

        decimal CapacityKg { get; }
    }
}
