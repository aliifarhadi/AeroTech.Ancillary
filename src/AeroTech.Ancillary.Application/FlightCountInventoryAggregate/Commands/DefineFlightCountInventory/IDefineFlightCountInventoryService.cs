namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory
{
    public interface IDefineFlightCountInventoryService
    {
        Task<FlightCountInventoryResult> DefineAsync(IDefineFlightCountInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
