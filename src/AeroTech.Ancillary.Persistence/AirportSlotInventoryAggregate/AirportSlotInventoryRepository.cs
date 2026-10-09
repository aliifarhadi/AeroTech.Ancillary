using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AeroTech.Ancillary.Persistence.AirportSlotInventoryAggregate
{
    public sealed class AirportSlotInventoryRepository : IAirportSlotInventoryRepository
    {
        private static readonly TimeSpan FacilityLockWait = TimeSpan.FromSeconds(20);

        private readonly AncillaryDbContext _dbContext;

        public AirportSlotInventoryRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(AirportSlotInventory inventory, CancellationToken cancellationToken = default)
            => await _dbContext.AirportSlotInventories.AddAsync(inventory, cancellationToken);

        public Task<AirportSlotInventory?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.AirportSlotInventories
                .Include(inventory => inventory.Adjustments)
                .FirstOrDefaultAsync(inventory => inventory.Id == id, cancellationToken);

        public async Task<IAsyncDisposable> LockFacilityAsync(int ownerAirlineId, long facilityId, CancellationToken cancellationToken = default)
        {
            var resource = $"Ancillary.AirportSlot.{ownerAirlineId}.{facilityId}";
            var deadline = DateTime.UtcNow + FacilityLockWait;

            while (!await TryLockAsync(resource, cancellationToken))
            {
                if (DateTime.UtcNow >= deadline)
                    throw ExceptionFactory.InventoryFacilityBusy();

                await Task.Delay(Random.Shared.Next(15, 45), cancellationToken);
            }

            return new FacilityLock(_dbContext, resource);
        }

        public Task<bool> HasOverlapAsync(
            int ownerAirlineId,
            long facilityId,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default)
            => _dbContext.AirportSlotInventories.AnyAsync(
                inventory => inventory.OwnerAirlineId == ownerAirlineId
                             && inventory.FacilityId == facilityId
                             && inventory.Status != InventoryRecordStatus.Retired
                             && inventory.StartUtc < endUtc
                             && startUtc < inventory.EndUtc,
                cancellationToken);

        private async Task<bool> TryLockAsync(string resource, CancellationToken cancellationToken)
        {
            var database = _dbContext.Database;
            var outcome = new SqlParameter("@outcome", SqlDbType.Int) { Direction = ParameterDirection.Output };

            await database.OpenConnectionAsync(cancellationToken);

            try
            {
                await database.ExecuteSqlRawAsync(
                    "EXEC @outcome = sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 0",
                    [outcome, new SqlParameter("@resource", resource)],
                    cancellationToken);
            }
            catch
            {
                await database.CloseConnectionAsync();
                throw;
            }

            if ((int)outcome.Value >= 0)
                return true;

            await database.CloseConnectionAsync();

            return false;
        }

        private sealed class FacilityLock : IAsyncDisposable
        {
            private readonly AncillaryDbContext _dbContext;
            private readonly string _resource;

            public FacilityLock(AncillaryDbContext dbContext, string resource)
            {
                _dbContext = dbContext;
                _resource = resource;
            }

            public async ValueTask DisposeAsync()
            {
                try
                {
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        "EXEC sp_releaseapplock @Resource = @resource, @LockOwner = 'Session'",
                        new SqlParameter("@resource", _resource));
                }
                finally
                {
                    await _dbContext.Database.CloseConnectionAsync();
                }
            }
        }
    }
}
