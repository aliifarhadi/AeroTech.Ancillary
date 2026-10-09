using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoryById
{
    public sealed class GetFlightWeightInventoryByIdService : IGetFlightWeightInventoryByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IInventoryCallerScope _scope;

        public GetFlightWeightInventoryByIdService(AncillaryQueryDbContext dbContext, IInventoryCallerScope scope)
        {
            _dbContext = dbContext;
            _scope = scope;
        }

        public async Task<BackofficeFlightWeightInventoryDto> ExecuteAsync(long inventoryId, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var inventory = await _dbContext.FlightWeightInventories
                                .AsNoTracking()
                                .FirstOrDefaultAsync(row => row.Id == inventoryId && row.OwnerAirlineId == ownerAirlineId, cancellationToken)
                            ?? throw ExceptionFactory.InventorySourceNotFound();
            var adjustments = await _dbContext.FlightWeightAdjustments
                .AsNoTracking()
                .Where(row => row.FlightWeightInventoryId == inventory.Id)
                .ToListAsync(cancellationToken);

            return FlightWeightInventoryMapper.ToBackofficeInventory(inventory, adjustments);
        }
    }
}
