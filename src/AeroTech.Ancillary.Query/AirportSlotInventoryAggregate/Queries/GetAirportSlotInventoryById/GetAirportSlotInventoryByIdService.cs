using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoryById
{
    public sealed class GetAirportSlotInventoryByIdService : IGetAirportSlotInventoryByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IInventoryCallerScope _scope;

        public GetAirportSlotInventoryByIdService(AncillaryQueryDbContext dbContext, IInventoryCallerScope scope)
        {
            _dbContext = dbContext;
            _scope = scope;
        }

        public async Task<BackofficeAirportSlotInventoryDto> ExecuteAsync(long inventoryId, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var inventory = await _dbContext.AirportSlotInventories
                                .AsNoTracking()
                                .FirstOrDefaultAsync(row => row.Id == inventoryId && row.OwnerAirlineId == ownerAirlineId, cancellationToken)
                            ?? throw ExceptionFactory.InventorySourceNotFound();
            var adjustments = await _dbContext.AirportSlotAdjustments
                .AsNoTracking()
                .Where(row => row.AirportSlotInventoryId == inventory.Id)
                .ToListAsync(cancellationToken);

            return AirportSlotInventoryMapper.ToBackofficeInventory(inventory, adjustments);
        }
    }
}
