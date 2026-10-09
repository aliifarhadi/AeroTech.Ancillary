using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Domain._Shared.Rules;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.FlightCountInventoryAggregate
{
    public sealed class FlightCountInventory : AggregateRoot<long>
    {
        private readonly List<FlightCountAdjustment> _adjustments = new();

        private FlightCountInventory()
        {
        }

        private FlightCountInventory(
            long id,
            int ownerAirlineId,
            long flightId,
            long resourceId,
            InventoryCountUnit countUnit,
            int totalCapacity,
            DateTimeOffset createdAt)
        {
            Id = id;
            OwnerAirlineId = ownerAirlineId;
            FlightId = flightId;
            ResourceId = resourceId;
            CountUnit = countUnit;
            TotalCapacity = totalCapacity;
            Status = InventoryRecordStatus.Draft;
            Version = 1;
            CreatedAt = createdAt;
            UpdatedAt = createdAt;
        }

        public int OwnerAirlineId { get; private set; }

        public long FlightId { get; private set; }

        public long ResourceId { get; private set; }

        public InventoryCountUnit CountUnit { get; private set; }

        public int TotalCapacity { get; private set; }

        public bool ClosedForSale { get; private set; }

        public InventoryRecordStatus Status { get; private set; }

        public long Version { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public IReadOnlyCollection<FlightCountAdjustment> Adjustments => _adjustments.AsReadOnly();

        public static FlightCountInventory Define(
            long id,
            int ownerAirlineId,
            long flightId,
            long resourceId,
            InventoryCountUnit countUnit,
            int totalCapacity,
            DateTimeOffset now)
        {
            Require(ownerAirlineId > 0, nameof(OwnerAirlineId));
            Require(flightId > 0, nameof(FlightId));
            Require(resourceId > 0, nameof(ResourceId));
            Require(Enum.IsDefined(countUnit), nameof(CountUnit));
            Require(totalCapacity >= 0, nameof(TotalCapacity));

            return new FlightCountInventory(id, ownerAirlineId, flightId, resourceId, countUnit, totalCapacity, now);
        }

        public void Activate(FlightSourceEvidence evidence, long expectedVersion, DateTimeOffset now)
        {
            EnsureStatus(InventoryRecordStatus.Draft, InventoryRecordStatus.Suspended);
            EnsureVersion(expectedVersion);
            InventoryRules.EnsureVerified(evidence.Flight, $"Flight {FlightId}");
            InventoryRules.EnsureVerified(evidence.Resource, $"Count resource {ResourceId}");

            if (evidence.CountUnitOfLiveSources is { } unit && unit != CountUnit)
                throw ExceptionFactory.InventoryUnitMismatch($"resource {ResourceId} is counted in {unit}");

            Status = InventoryRecordStatus.Active;
            Touch(now);
        }

        public void Suspend(long expectedVersion, DateTimeOffset now)
        {
            EnsureStatus(InventoryRecordStatus.Active);
            EnsureVersion(expectedVersion);
            Status = InventoryRecordStatus.Suspended;
            Touch(now);
        }

        public void Retire(long expectedVersion, DateTimeOffset now)
        {
            EnsureLive();
            EnsureVersion(expectedVersion);
            Status = InventoryRecordStatus.Retired;
            Touch(now);
        }

        public void CloseForSale(long expectedVersion, DateTimeOffset now)
        {
            EnsureLive();
            EnsureVersion(expectedVersion);

            if (ClosedForSale)
                throw ExceptionFactory.InventorySourceStatusChangeNotAllowed();

            ClosedForSale = true;
            Touch(now);
        }

        public void OpenForSale(long expectedVersion, DateTimeOffset now)
        {
            EnsureLive();
            EnsureVersion(expectedVersion);

            if (!ClosedForSale)
                throw ExceptionFactory.InventorySourceStatusChangeNotAllowed();

            ClosedForSale = false;
            Touch(now);
        }

        public FlightCountAdjustment AdjustTo(
            int newTotal,
            string reasonCode,
            long actorId,
            string correlationId,
            long expectedVersion,
            IIdGenerator idGenerator,
            DateTimeOffset now)
        {
            EnsureLive();
            Require(newTotal >= 0, nameof(TotalCapacity));
            Require(InventoryRules.IsCode(reasonCode, InventoryRules.ReasonCodeMaxLength), nameof(FlightCountAdjustment.ReasonCode));
            Require(actorId > 0, nameof(FlightCountAdjustment.ActorId));
            Require(InventoryRules.IsCode(correlationId, InventoryRules.CorrelationIdMaxLength), nameof(FlightCountAdjustment.CorrelationId));

            var replayed = _adjustments.FirstOrDefault(adjustment => adjustment.CorrelationId == correlationId);

            if (replayed is not null)
            {
                return replayed.IsRequest(newTotal, reasonCode, actorId, expectedVersion)
                    ? replayed
                    : throw ExceptionFactory.InventoryCorrelationConflict(correlationId);
            }

            EnsureVersion(expectedVersion);
            Require(newTotal != TotalCapacity, nameof(TotalCapacity));

            var adjustment = new FlightCountAdjustment(
                idGenerator.NewId(),
                Id,
                TotalCapacity,
                newTotal,
                reasonCode,
                actorId,
                correlationId,
                now,
                expectedVersion,
                expectedVersion + 1);

            TotalCapacity = newTotal;
            _adjustments.Add(adjustment);
            Touch(now);

            return adjustment;
        }

        private void EnsureLive() => EnsureStatus(InventoryRecordStatus.Draft, InventoryRecordStatus.Active, InventoryRecordStatus.Suspended);

        private void EnsureStatus(params InventoryRecordStatus[] allowed)
        {
            if (!allowed.Contains(Status))
                throw ExceptionFactory.InventorySourceStatusChangeNotAllowed();
        }

        private void EnsureVersion(long expectedVersion)
        {
            if (expectedVersion != Version)
                throw ExceptionFactory.InventoryVersionConflict();
        }

        private void Touch(DateTimeOffset now)
        {
            Version++;
            UpdatedAt = now;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.InventorySourceIsInvalid($"{nameof(FlightCountInventory)}.{field}");
        }
    }
}
