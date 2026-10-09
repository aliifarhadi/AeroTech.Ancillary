namespace AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts
{
    public interface IFlightWeightInventoryRepository
    {
        Task AddAsync(FlightWeightInventory inventory, CancellationToken cancellationToken = default);

        Task<FlightWeightInventory?> GetAsync(long id, CancellationToken cancellationToken = default);
    }
}
