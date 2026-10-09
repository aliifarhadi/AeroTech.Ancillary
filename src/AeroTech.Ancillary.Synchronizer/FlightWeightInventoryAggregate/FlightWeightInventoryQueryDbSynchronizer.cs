using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.FlightWeightInventoryAggregate
{
    public sealed class FlightWeightInventoryQueryDbSynchronizer : IFlightWeightInventoryQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IClock _clock;

        public FlightWeightInventoryQueryDbSynchronizer(AncillaryQueryDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public async Task ProjectAsync(FlightWeightInventoryReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var inventory = await _dbContext.FlightWeightInventories.FirstOrDefaultAsync(row => row.Id == snapshot.InventoryId, cancellationToken);

            if (inventory is null)
            {
                inventory = new FlightWeightInventoryReadModel { Id = snapshot.InventoryId };
                _dbContext.FlightWeightInventories.Add(inventory);
            }

            inventory.OwnerAirlineId = snapshot.OwnerAirlineId;
            inventory.FlightId = snapshot.FlightId;
            inventory.WeightResourceId = snapshot.WeightResourceId;
            inventory.CapacityKg = snapshot.CapacityKg;
            inventory.ClosedForSale = snapshot.ClosedForSale;
            inventory.Status = snapshot.Status;
            inventory.Version = snapshot.Version;
            inventory.CreatedAt = snapshot.CreatedAt;
            inventory.UpdatedAt = snapshot.UpdatedAt;
            inventory.AdjustmentCount = snapshot.Adjustments.Count;
            inventory.LastUpdateTime = _clock.GetDateTime();

            var storedIds = await _dbContext.FlightWeightAdjustments
                .Where(row => row.FlightWeightInventoryId == snapshot.InventoryId)
                .Select(row => row.Id)
                .ToListAsync(cancellationToken);

            foreach (var adjustment in snapshot.Adjustments.Where(adjustment => !storedIds.Contains(adjustment.AdjustmentId)))
            {
                _dbContext.FlightWeightAdjustments.Add(new FlightWeightAdjustmentReadModel
                {
                    Id = adjustment.AdjustmentId,
                    FlightWeightInventoryId = snapshot.InventoryId,
                    PreviousKg = adjustment.PreviousKg,
                    NewKg = adjustment.NewKg,
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
