using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Domain._Shared.Rules;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate
{
    public sealed class AirportSlotInventory : AggregateRoot<long>
    {
        private static readonly TimeSpan MaxDuration = TimeSpan.FromHours(24);

        private static bool IsUtcMinute(DateTimeOffset value) => value.Offset == TimeSpan.Zero && value.Ticks % TimeSpan.TicksPerMinute == 0;

        private readonly List<AirportSlotAdjustment> _adjustments = new();

        private AirportSlotInventory()
        {
        }

        private AirportSlotInventory(
            long id,
            int ownerAirlineId,
            int airportId,
            long facilityId,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            int capacityPersons,
            DateTimeOffset createdAt)
        {
            Id = id;
            OwnerAirlineId = ownerAirlineId;
            AirportId = airportId;
            FacilityId = facilityId;
            StartUtc = startUtc;
            EndUtc = endUtc;
            CapacityPersons = capacityPersons;
            Status = InventoryRecordStatus.Draft;
            Version = 1;
            CreatedAt = createdAt;
            UpdatedAt = createdAt;
        }

        public int OwnerAirlineId { get; private set; }

        public int AirportId { get; private set; }

        public long FacilityId { get; private set; }

        public DateTimeOffset StartUtc { get; private set; }

        public DateTimeOffset EndUtc { get; private set; }

        public int CapacityPersons { get; private set; }

        public bool ClosedForSale { get; private set; }

        public InventoryRecordStatus Status { get; private set; }

        public long Version { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public IReadOnlyCollection<AirportSlotAdjustment> Adjustments => _adjustments.AsReadOnly();

        public static AirportSlotInventory Define(
            long id,
            int ownerAirlineId,
            int airportId,
            long facilityId,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            int capacityPersons,
            DateTimeOffset now)
        {
            Require(ownerAirlineId > 0, nameof(OwnerAirlineId));
            Require(airportId > 0, nameof(AirportId));
            Require(facilityId > 0, nameof(FacilityId));
            Require(IsUtcMinute(startUtc), nameof(StartUtc));
            Require(IsUtcMinute(endUtc), nameof(EndUtc));
            Require(startUtc < endUtc && endUtc - startUtc <= MaxDuration, nameof(EndUtc));
            Require(capacityPersons >= 0, nameof(CapacityPersons));

            return new AirportSlotInventory(id, ownerAirlineId, airportId, facilityId, startUtc, endUtc, capacityPersons, now);
        }

        public void Activate(AirportSlotEvidence evidence, long expectedVersion, DateTimeOffset now)
        {
            EnsureStatus(InventoryRecordStatus.Draft, InventoryRecordStatus.Suspended);
            EnsureVersion(expectedVersion);
            if (!evidence.AirportExists)
                throw ExceptionFactory.InventoryReferenceNotFound($"Airport {AirportId}");

            InventoryRules.EnsureVerified(evidence.Facility, $"Airport facility {FacilityId}");

            if (evidence.FacilityAirportId != AirportId)
                throw ExceptionFactory.InventoryReferenceNotFound($"Airport facility {FacilityId} at airport {AirportId}");

            if (string.IsNullOrWhiteSpace(evidence.FacilityTimeZoneId) || !TimeZoneInfo.TryFindSystemTimeZoneById(evidence.FacilityTimeZoneId, out _))
                throw ExceptionFactory.InventoryReferenceNotFound($"Time zone of airport facility {FacilityId}");

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

        public AirportSlotAdjustment AdjustTo(
            int newTotal,
            string reasonCode,
            long actorId,
            string correlationId,
            long expectedVersion,
            IIdGenerator idGenerator,
            DateTimeOffset now)
        {
            EnsureLive();
            Require(newTotal >= 0, nameof(CapacityPersons));
            Require(InventoryRules.IsCode(reasonCode, InventoryRules.ReasonCodeMaxLength), nameof(AirportSlotAdjustment.ReasonCode));
            Require(actorId > 0, nameof(AirportSlotAdjustment.ActorId));
            Require(InventoryRules.IsCode(correlationId, InventoryRules.CorrelationIdMaxLength), nameof(AirportSlotAdjustment.CorrelationId));

            var replayed = _adjustments.FirstOrDefault(adjustment => adjustment.CorrelationId == correlationId);

            if (replayed is not null)
            {
                return replayed.IsRequest(newTotal, reasonCode, actorId, expectedVersion)
                    ? replayed
                    : throw ExceptionFactory.InventoryCorrelationConflict(correlationId);
            }

            EnsureVersion(expectedVersion);
            Require(newTotal != CapacityPersons, nameof(CapacityPersons));

            var adjustment = new AirportSlotAdjustment(
                idGenerator.NewId(),
                Id,
                CapacityPersons,
                newTotal,
                reasonCode,
                actorId,
                correlationId,
                now,
                expectedVersion,
                expectedVersion + 1);

            CapacityPersons = newTotal;
            _adjustments.Add(adjustment);
            Touch(now);

            return adjustment;
        }

        public bool Overlaps(int ownerAirlineId, long facilityId, DateTimeOffset startUtc, DateTimeOffset endUtc)
            => Status != InventoryRecordStatus.Retired
               && OwnerAirlineId == ownerAirlineId
               && FacilityId == facilityId
               && StartUtc < endUtc
               && startUtc < EndUtc;

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
                throw ExceptionFactory.InventorySourceIsInvalid($"{nameof(AirportSlotInventory)}.{field}");
        }
    }
}
