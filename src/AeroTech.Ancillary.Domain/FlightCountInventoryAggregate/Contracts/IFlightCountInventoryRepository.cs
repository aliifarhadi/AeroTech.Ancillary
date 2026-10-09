using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts
{
    public interface IFlightCountInventoryRepository
    {
        Task AddAsync(FlightCountInventory inventory, CancellationToken cancellationToken = default);

        Task<FlightCountInventory?> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<InventoryCountUnit?> FindCountUnitOfLiveSourcesAsync(
            int ownerAirlineId,
            long resourceId,
            long? exceptInventoryId,
            CancellationToken cancellationToken = default);
    }
}
