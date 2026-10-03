using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ServiceReservationAggregate
{
    public sealed class ServiceReservation : AggregateRoot<long>
    {
        private const int IdempotencyKeyMaxLength = 128;
        private const int ReferenceMaxLength = 128;

        private readonly List<ServiceReservationUnit> _units = new();

        private ServiceReservation()
        {
        }

        private ServiceReservation(long id, string idempotencyKey, string reference, DateTimeOffset expiresAt, DateTimeOffset createdAt)
        {
            Id = id;
            IdempotencyKey = idempotencyKey;
            Reference = reference;
            ExpiresAt = expiresAt;
            CreatedAt = createdAt;
        }

        public string IdempotencyKey { get; private set; } = default!;

        public string Reference { get; private set; } = default!;

        public DateTimeOffset ExpiresAt { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public IReadOnlyCollection<ServiceReservationUnit> Units => _units.AsReadOnly();

        public static void EnsureReservable(DateTimeOffset expiresAt, IReadOnlyCollection<string> unitReferences, DateTimeOffset now)
        {
            Require(expiresAt > now, nameof(ExpiresAt));
            Require(unitReferences.Count > 0, nameof(Units));
            Require(
                unitReferences.Distinct(StringComparer.Ordinal).Count() == unitReferences.Count,
                $"{nameof(ServiceReservationUnit)}.{nameof(ServiceReservationUnit.UnitReference)}");
        }

        public static ServiceReservation Reserve(
            long id,
            string idempotencyKey,
            string reference,
            DateTimeOffset expiresAt,
            IReadOnlyList<ServiceReservationUnitArgs> units,
            IIdGenerator idGenerator,
            DateTimeOffset now)
        {
            Require(idempotencyKey is { Length: >= 1 and <= IdempotencyKeyMaxLength }, nameof(IdempotencyKey));
            Require(reference is { Length: >= 1 and <= ReferenceMaxLength }, nameof(Reference));
            EnsureReservable(expiresAt, units.Select(unit => unit.UnitReference).ToList(), now);

            var reservation = new ServiceReservation(id, idempotencyKey, reference, expiresAt, now);

            reservation._units.AddRange(units.Select(unit => new ServiceReservationUnit(idGenerator.NewId(), id, unit)));

            return reservation;
        }

        public void EnsureSameContent(string reference, DateTimeOffset expiresAt, IReadOnlyList<ServiceReservationUnitSelection> units)
        {
            var same = reference == Reference
                       && expiresAt == ExpiresAt
                       && units.Count == _units.Count
                       && _units.All(unit => units.Any(unit.IsSelectedBy));

            if (!same)
                throw ExceptionFactory.ServiceReservationIdempotencyKeyReused();
        }

        public ServiceReservationUnitStatus StatusOf(ServiceReservationUnit unit, DateTimeOffset now)
            => unit.Status == ServiceReservationUnitStatus.Held && now >= ExpiresAt
                ? ServiceReservationUnitStatus.Expired
                : unit.Status;

        public bool Confirm(DateTimeOffset now)
        {
            var statuses = StatusesOf(_units, now);

            if (statuses.All(status => status == ServiceReservationUnitStatus.Held))
                return ChangeStatus(_units, ServiceReservationUnitStatus.Confirmed);

            if (statuses.All(status => status == ServiceReservationUnitStatus.Confirmed))
                return false;

            if (statuses.Contains(ServiceReservationUnitStatus.Expired))
                throw ExceptionFactory.ServiceReservationHasExpired();

            if (statuses.Contains(ServiceReservationUnitStatus.Released))
                throw ExceptionFactory.ServiceReservationWasReleased();

            if (statuses.Contains(ServiceReservationUnitStatus.Cancelled))
                throw ExceptionFactory.ServiceReservationWasCancelled();

            throw ExceptionFactory.ServiceReservationIsInMixedState();
        }

        public bool Release(DateTimeOffset now)
        {
            var statuses = StatusesOf(_units, now);

            if (statuses.All(status => status == ServiceReservationUnitStatus.Held))
                return ChangeStatus(_units, ServiceReservationUnitStatus.Released);

            if (statuses.All(status => status == ServiceReservationUnitStatus.Released))
                return false;

            if (statuses.Contains(ServiceReservationUnitStatus.Confirmed))
                throw ExceptionFactory.ServiceReservationIsAlreadyConfirmed();

            if (statuses.Contains(ServiceReservationUnitStatus.Expired))
                throw ExceptionFactory.ServiceReservationHasExpired();

            if (statuses.Contains(ServiceReservationUnitStatus.Cancelled))
                throw ExceptionFactory.ServiceReservationWasCancelled();

            throw ExceptionFactory.ServiceReservationIsInMixedState();
        }

        public bool Cancel(IReadOnlyCollection<long> unitIds, DateTimeOffset now)
        {
            Require(unitIds.Count > 0, nameof(Units));

            var selected = unitIds
                .Distinct()
                .Select(unitId => _units.FirstOrDefault(unit => unit.Id == unitId)
                                  ?? throw ExceptionFactory.ServiceReservationRequestIsInvalid(nameof(Units)))
                .ToList();
            var statuses = StatusesOf(selected, now);

            if (statuses.All(status => status == ServiceReservationUnitStatus.Confirmed))
                return ChangeStatus(selected, ServiceReservationUnitStatus.Cancelled);

            if (statuses.All(status => status == ServiceReservationUnitStatus.Cancelled))
                return false;

            if (statuses.Contains(ServiceReservationUnitStatus.Held))
                throw ExceptionFactory.ServiceReservationIsNotConfirmed();

            if (statuses.Contains(ServiceReservationUnitStatus.Released))
                throw ExceptionFactory.ServiceReservationWasReleased();

            if (statuses.Contains(ServiceReservationUnitStatus.Expired))
                throw ExceptionFactory.ServiceReservationHasExpired();

            throw ExceptionFactory.ServiceReservationIsInMixedState();
        }

        private List<ServiceReservationUnitStatus> StatusesOf(IEnumerable<ServiceReservationUnit> units, DateTimeOffset now)
            => units.Select(unit => StatusOf(unit, now)).ToList();

        private static bool ChangeStatus(IEnumerable<ServiceReservationUnit> units, ServiceReservationUnitStatus status)
        {
            foreach (var unit in units)
                unit.ChangeStatus(status);

            return true;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ServiceReservationRequestIsInvalid(field);
        }
    }
}
