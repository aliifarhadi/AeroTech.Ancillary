using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoryById
{
    public sealed class GetFlightCountInventoryByIdService : IGetFlightCountInventoryByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IInventoryCallerScope _scope;

        public GetFlightCountInventoryByIdService(AncillaryQueryDbContext dbContext, IInventoryCallerScope scope)
        {
            _dbContext = dbContext;
            _scope = scope;
        }

        public async Task<BackofficeFlightCountInventoryDto> ExecuteAsync(long inventoryId, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var inventory = await _dbContext.FlightCountInventories
                                .AsNoTracking()
                                .FirstOrDefaultAsync(row => row.Id == inventoryId && row.OwnerAirlineId == ownerAirlineId, cancellationToken)
                            ?? throw ExceptionFactory.InventorySourceNotFound();
            var adjustments = await _dbContext.FlightCountAdjustments
                .AsNoTracking()
                .Where(row => row.FlightCountInventoryId == inventory.Id)
                .ToListAsync(cancellationToken);

            return FlightCountInventoryMapper.ToBackofficeInventory(inventory, adjustments);
        }
    }
}
