using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.FlightCountInventoryAggregate
{
    public sealed class FlightCountInventoryQueryDbSynchronizer : IFlightCountInventoryQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IClock _clock;

        public FlightCountInventoryQueryDbSynchronizer(AncillaryQueryDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public async Task ProjectAsync(FlightCountInventoryReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var inventory = await _dbContext.FlightCountInventories.FirstOrDefaultAsync(row => row.Id == snapshot.InventoryId, cancellationToken);

            if (inventory is null)
            {
                inventory = new FlightCountInventoryReadModel { Id = snapshot.InventoryId };
                _dbContext.FlightCountInventories.Add(inventory);
            }

            inventory.OwnerAirlineId = snapshot.OwnerAirlineId;
            inventory.FlightId = snapshot.FlightId;
            inventory.ResourceId = snapshot.ResourceId;
            inventory.CountUnit = snapshot.CountUnit;
            inventory.TotalCapacity = snapshot.TotalCapacity;
            inventory.ClosedForSale = snapshot.ClosedForSale;
            inventory.Status = snapshot.Status;
            inventory.Version = snapshot.Version;
            inventory.CreatedAt = snapshot.CreatedAt;
            inventory.UpdatedAt = snapshot.UpdatedAt;
            inventory.AdjustmentCount = snapshot.Adjustments.Count;
            inventory.LastUpdateTime = _clock.GetDateTime();

            var storedIds = await _dbContext.FlightCountAdjustments
                .Where(row => row.FlightCountInventoryId == snapshot.InventoryId)
                .Select(row => row.Id)
                .ToListAsync(cancellationToken);

            foreach (var adjustment in snapshot.Adjustments.Where(adjustment => !storedIds.Contains(adjustment.AdjustmentId)))
            {
                _dbContext.FlightCountAdjustments.Add(new FlightCountAdjustmentReadModel
                {
                    Id = adjustment.AdjustmentId,
                    FlightCountInventoryId = snapshot.InventoryId,
                    PreviousTotal = adjustment.PreviousTotal,
                    NewTotal = adjustment.NewTotal,
                    ReasonCode = adjustment.ReasonCode,
                    ActorId = adjustment.ActorId,
                    CorrelationId = adjustment.CorrelationId,
                    OccurredAt = adjustment.OccurredAt,
                    ExpectedVersion = adjustment.ExpectedVersion,
                    ResultingVersion = adjustment.ResultingVersion,
                });
            }
        }
    }
}
