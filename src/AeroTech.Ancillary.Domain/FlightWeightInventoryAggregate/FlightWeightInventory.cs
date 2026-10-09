using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Domain._Shared.Rules;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate
{
    public sealed class FlightWeightInventory : AggregateRoot<long>
    {
        private readonly List<FlightWeightAdjustment> _adjustments = new();

        private FlightWeightInventory()
        {
        }

        private FlightWeightInventory(
            long id,
            int ownerAirlineId,
            long flightId,
            long weightResourceId,
            decimal capacityKg,
            DateTimeOffset createdAt)
        {
            Id = id;
            OwnerAirlineId = ownerAirlineId;
            FlightId = flightId;
            WeightResourceId = weightResourceId;
            CapacityKg = capacityKg;
            Status = InventoryRecordStatus.Draft;
            Version = 1;
            CreatedAt = createdAt;
            UpdatedAt = createdAt;
        }

        public int OwnerAirlineId { get; private set; }

        public long FlightId { get; private set; }

        public long WeightResourceId { get; private set; }

        public decimal CapacityKg { get; private set; }

        public bool ClosedForSale { get; private set; }

        public InventoryRecordStatus Status { get; private set; }

        public long Version { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public IReadOnlyCollection<FlightWeightAdjustment> Adjustments => _adjustments.AsReadOnly();

        public static FlightWeightInventory Define(
            long id,
            int ownerAirlineId,
            long flightId,
            long weightResourceId,
            decimal capacityKg,
            DateTimeOffset now)
        {
            Require(ownerAirlineId > 0, nameof(OwnerAirlineId));
            Require(flightId > 0, nameof(FlightId));
            Require(weightResourceId > 0, nameof(WeightResourceId));
            Require(InventoryRules.IsKg(capacityKg), nameof(CapacityKg));

            return new FlightWeightInventory(id, ownerAirlineId, flightId, weightResourceId, capacityKg, now);
        }

        public void Activate(FlightSourceEvidence evidence, long expectedVersion, DateTimeOffset now)
        {
            EnsureStatus(InventoryRecordStatus.Draft, InventoryRecordStatus.Suspended);
            EnsureVersion(expectedVersion);
            InventoryRules.EnsureVerified(evidence.Flight, $"Flight {FlightId}");
            InventoryRules.EnsureVerified(evidence.Resource, $"Weight resource {WeightResourceId}");

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

        public FlightWeightAdjustment AdjustTo(
            decimal newKg,
            string reasonCode,
            long actorId,
            string correlationId,
            long expectedVersion,
            IIdGenerator idGenerator,
            DateTimeOffset now)
        {
            EnsureLive();
            Require(InventoryRules.IsKg(newKg), nameof(CapacityKg));
            Require(InventoryRules.IsCode(reasonCode, InventoryRules.ReasonCodeMaxLength), nameof(FlightWeightAdjustment.ReasonCode));
            Require(actorId > 0, nameof(FlightWeightAdjustment.ActorId));
            Require(InventoryRules.IsCode(correlationId, InventoryRules.CorrelationIdMaxLength), nameof(FlightWeightAdjustment.CorrelationId));

            var replayed = _adjustments.FirstOrDefault(adjustment => adjustment.CorrelationId == correlationId);

            if (replayed is not null)
            {
                return replayed.IsRequest(newKg, reasonCode, actorId, expectedVersion)
                    ? replayed
                    : throw ExceptionFactory.InventoryCorrelationConflict(correlationId);
            }

            EnsureVersion(expectedVersion);
            Require(newKg != CapacityKg, nameof(CapacityKg));

            var adjustment = new FlightWeightAdjustment(
                idGenerator.NewId(),
                Id,
                CapacityKg,
                newKg,
                reasonCode,
                actorId,
                correlationId,
                now,
                expectedVersion,
                expectedVersion + 1);

            CapacityKg = newKg;
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
                throw ExceptionFactory.InventorySourceIsInvalid($"{nameof(FlightWeightInventory)}.{field}");
        }
    }
}
