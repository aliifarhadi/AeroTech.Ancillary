using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.FlightCountInventoryAggregate
{
    public sealed class FlightCountInventoryRepository : IFlightCountInventoryRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public FlightCountInventoryRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(FlightCountInventory inventory, CancellationToken cancellationToken = default)
            => await _dbContext.FlightCountInventories.AddAsync(inventory, cancellationToken);

        public Task<FlightCountInventory?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.FlightCountInventories
                .Include(inventory => inventory.Adjustments)
                .FirstOrDefaultAsync(inventory => inventory.Id == id, cancellationToken);

        public async Task<InventoryCountUnit?> FindCountUnitOfLiveSourcesAsync(
            int ownerAirlineId,
            long resourceId,
            long? exceptInventoryId,
            CancellationToken cancellationToken = default)
            => await _dbContext.FlightCountInventories
                .Where(inventory => inventory.OwnerAirlineId == ownerAirlineId
                                    && inventory.ResourceId == resourceId
                                    && inventory.Status != InventoryRecordStatus.Retired
                                    && (exceptInventoryId == null || inventory.Id != exceptInventoryId))
                .OrderBy(inventory => inventory.Id)
                .Select(inventory => (InventoryCountUnit?)inventory.CountUnit)
                .FirstOrDefaultAsync(cancellationToken);
    }
}
