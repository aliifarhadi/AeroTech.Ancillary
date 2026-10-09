using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.AirportSlotInventoryAggregate
{
    public sealed class AirportSlotInventoryQueryDbSynchronizer : IAirportSlotInventoryQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IClock _clock;

        public AirportSlotInventoryQueryDbSynchronizer(AncillaryQueryDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public async Task ProjectAsync(AirportSlotInventoryReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var inventory = await _dbContext.AirportSlotInventories.FirstOrDefaultAsync(row => row.Id == snapshot.InventoryId, cancellationToken);

            if (inventory is null)
            {
                inventory = new AirportSlotInventoryReadModel { Id = snapshot.InventoryId };
                _dbContext.AirportSlotInventories.Add(inventory);
            }

            inventory.OwnerAirlineId = snapshot.OwnerAirlineId;
            inventory.AirportId = snapshot.AirportId;
            inventory.FacilityId = snapshot.FacilityId;
            inventory.StartUtc = snapshot.StartUtc;
            inventory.EndUtc = snapshot.EndUtc;
            inventory.CapacityPersons = snapshot.CapacityPersons;
            inventory.ClosedForSale = snapshot.ClosedForSale;
            inventory.Status = snapshot.Status;
            inventory.Version = snapshot.Version;
            inventory.CreatedAt = snapshot.CreatedAt;
            inventory.UpdatedAt = snapshot.UpdatedAt;
            inventory.AdjustmentCount = snapshot.Adjustments.Count;
            inventory.LastUpdateTime = _clock.GetDateTime();

            var storedIds = await _dbContext.AirportSlotAdjustments
                .Where(row => row.AirportSlotInventoryId == snapshot.InventoryId)
                .Select(row => row.Id)
                .ToListAsync(cancellationToken);

            foreach (var adjustment in snapshot.Adjustments.Where(adjustment => !storedIds.Contains(adjustment.AdjustmentId)))
            {
                _dbContext.AirportSlotAdjustments.Add(new AirportSlotAdjustmentReadModel
                {
                    Id = adjustment.AdjustmentId,
                    AirportSlotInventoryId = snapshot.InventoryId,
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
