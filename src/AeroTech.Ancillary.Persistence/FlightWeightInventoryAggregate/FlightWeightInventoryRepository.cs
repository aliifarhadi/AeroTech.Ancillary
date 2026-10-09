using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.FlightWeightInventoryAggregate
{
    public sealed class FlightWeightInventoryRepository : IFlightWeightInventoryRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public FlightWeightInventoryRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(FlightWeightInventory inventory, CancellationToken cancellationToken = default)
            => await _dbContext.FlightWeightInventories.AddAsync(inventory, cancellationToken);

        public Task<FlightWeightInventory?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.FlightWeightInventories
                .Include(inventory => inventory.Adjustments)
                .FirstOrDefaultAsync(inventory => inventory.Id == id, cancellationToken);
    }
}
