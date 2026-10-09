namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory
{
    public interface IDefineFlightWeightInventoryService
    {
        Task<FlightWeightInventoryResult> DefineAsync(IDefineFlightWeightInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
