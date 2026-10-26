using System.Data;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AeroTech.Ancillary.Persistence.AncillaryReservationAggregate
{
    public sealed class InventoryLedger : IInventoryLedger
    {
        private static readonly TimeSpan SourceLockWait = TimeSpan.FromSeconds(20);

        private readonly AncillaryDbContext _dbContext;

        public InventoryLedger(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task<IInventoryLedgerSession> OpenAsync(IReadOnlyCollection<InventorySourceKey> sources, CancellationToken cancellationToken = default)
        {
            var locked = sources
                .Distinct()
                .OrderBy(source => source.ResourceKind)
                .ThenBy(source => source.InventorySourceId)
                .ToList();
            var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var source in locked)
                    await LockAsync(source, cancellationToken);
            }
            catch
            {
                await transaction.DisposeAsync();
                throw;
            }

            return new Session(_dbContext, transaction, locked);
        }

        private async Task LockAsync(InventorySourceKey source, CancellationToken cancellationToken)
        {
            var outcome = new SqlParameter("@outcome", SqlDbType.Int) { Direction = ParameterDirection.Output };

            await _dbContext.Database.ExecuteSqlRawAsync(
                "EXEC @outcome = sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = @timeout",
                [
                    outcome,
                    new SqlParameter("@resource", $"Ancillary.Inventory.{source.ResourceKind}.{source.InventorySourceId}"),
                    new SqlParameter("@timeout", (int)SourceLockWait.TotalMilliseconds)
                ],
                cancellationToken);

            if ((int)outcome.Value < 0)
                throw ExceptionFactory.InventorySourceBusy(source.ResourceKind, source.InventorySourceId);
        }

        private sealed class Session : IInventoryLedgerSession
        {
            private readonly AncillaryDbContext _dbContext;
            private readonly IDbContextTransaction _transaction;
            private readonly IReadOnlyList<InventorySourceKey> _sources;

            public Session(AncillaryDbContext dbContext, IDbContextTransaction transaction, IReadOnlyList<InventorySourceKey> sources)
            {
                _dbContext = dbContext;
                _transaction = transaction;
                _sources = sources;
            }

            public async Task<IReadOnlyDictionary<InventorySourceKey, InventorySourceBalance>> BalancesAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
            {
                if (_sources.Count == 0)
                    return new Dictionary<InventorySourceKey, InventorySourceBalance>();

                var usage = await UsageAsync(now, cancellationToken);

                return (await SourcesAsync(cancellationToken)).ToDictionary(
                    source => source.Key,
                    source => new InventorySourceBalance(source.Capacity, source.Status, source.ClosedForSale, usage.GetValueOrDefault(source.Key)));
            }

            public Task CommitAsync(CancellationToken cancellationToken = default) => _transaction.CommitAsync(cancellationToken);

            public ValueTask DisposeAsync() => _transaction.DisposeAsync();

            private async Task<List<SourceRow>> SourcesAsync(CancellationToken cancellationToken)
            {
                var counts = IdsOf(InventoryResourceKind.FlightCount);
                var weights = IdsOf(InventoryResourceKind.FlightWeight);
                var slots = IdsOf(InventoryResourceKind.AirportSlot);
                var rows = new List<SourceRow>();

                if (counts.Count > 0)
                    rows.AddRange(await _dbContext.FlightCountInventories
                        .AsNoTracking()
                        .Where(source => counts.Contains(source.Id))
                        .Select(source => new SourceRow(new InventorySourceKey(InventoryResourceKind.FlightCount, source.Id), source.TotalCapacity, source.Status, source.ClosedForSale))
                        .ToListAsync(cancellationToken));

                if (weights.Count > 0)
                    rows.AddRange(await _dbContext.FlightWeightInventories
                        .AsNoTracking()
                        .Where(source => weights.Contains(source.Id))
                        .Select(source => new SourceRow(new InventorySourceKey(InventoryResourceKind.FlightWeight, source.Id), source.CapacityKg, source.Status, source.ClosedForSale))
                        .ToListAsync(cancellationToken));

                if (slots.Count > 0)
                    rows.AddRange(await _dbContext.AirportSlotInventories
                        .AsNoTracking()
                        .Where(source => slots.Contains(source.Id))
                        .Select(source => new SourceRow(new InventorySourceKey(InventoryResourceKind.AirportSlot, source.Id), source.CapacityPersons, source.Status, source.ClosedForSale))
                        .ToListAsync(cancellationToken));

                return rows;
            }

            private async Task<Dictionary<InventorySourceKey, decimal>> UsageAsync(DateTimeOffset now, CancellationToken cancellationToken)
            {
                var ids = _sources.Select(source => source.InventorySourceId).ToList();

                return (await _dbContext.AncillaryReservations
                        .AsNoTracking()
                        .SelectMany(reservation => reservation.Units
                            .Where(unit => unit.Status == AncillaryReservationUnitStatus.Confirmed
                                           || (unit.Status == AncillaryReservationUnitStatus.Held
                                               && (reservation.ExpiresAt == null || reservation.ExpiresAt > now)))
                            .SelectMany(unit => unit.Allocations))
                        .Where(allocation => ids.Contains(allocation.InventorySourceId))
                        .GroupBy(allocation => new { allocation.ResourceKind, allocation.InventorySourceId })
                        .Select(group => new { group.Key.ResourceKind, group.Key.InventorySourceId, Amount = group.Sum(allocation => allocation.Amount) })
                        .ToListAsync(cancellationToken))
                    .ToDictionary(row => new InventorySourceKey(row.ResourceKind, row.InventorySourceId), row => row.Amount);
            }

            private List<long> IdsOf(InventoryResourceKind kind)
                => _sources.Where(source => source.ResourceKind == kind).Select(source => source.InventorySourceId).ToList();

            private sealed record SourceRow(InventorySourceKey Key, decimal Capacity, InventoryRecordStatus Status, bool ClosedForSale);
        }
    }
}
