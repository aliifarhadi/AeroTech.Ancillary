using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate
{
    public sealed class AncillaryReservation : AggregateRoot<long>
    {
        private const int IdempotencyKeyMaxLength = 128;
        private const int ReferenceMaxLength = 128;

        private readonly List<AncillaryReservationUnit> _units = new();

        private AncillaryReservation()
        {
        }

        private AncillaryReservation(
            long id,
            string idempotencyKey,
            long orderId,
            string reference,
            DateTimeOffset? requestedExpiresAt,
            DateTimeOffset now)
        {
            Id = id;
            IdempotencyKey = idempotencyKey;
            OrderId = orderId;
            Reference = reference;
            RequestedExpiresAt = requestedExpiresAt;
            ExpiresAt = requestedExpiresAt;
            Status = AncillaryReservationStatus.Held;
            CreatedAt = now;
            UpdatedAt = now;
        }

        public string IdempotencyKey { get; private set; } = default!;

        public long OrderId { get; private set; }

        public string Reference { get; private set; } = default!;

        public DateTimeOffset? RequestedExpiresAt { get; private set; }

        public DateTimeOffset? ExpiresAt { get; private set; }

        public AncillaryReservationStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        public IReadOnlyCollection<AncillaryReservationUnit> Units => _units.AsReadOnly();

        public static AncillaryReservation Hold(
            long id,
            string idempotencyKey,
            long orderId,
            string reference,
            DateTimeOffset? requestedExpiresAt,
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            IIdGenerator idGenerator,
            DateTimeOffset now)
        {
            Require(idempotencyKey is { Length: >= 1 and <= IdempotencyKeyMaxLength }, nameof(IdempotencyKey));
            Require(orderId > 0, nameof(OrderId));
            Require(reference is { Length: >= 1 and <= ReferenceMaxLength }, nameof(Reference));
            Require(requestedExpiresAt is null || requestedExpiresAt > now, nameof(RequestedExpiresAt));
            Require(units.Count > 0, nameof(Units));
            Require(units.Select(unit => unit.OrderServiceId).Distinct().Count() == units.Count, nameof(AncillaryReservationUnit.OrderServiceId));

            var occurrences = units
                .Select(unit => unit.CoverageScope == ServiceCoverageScope.Order
                    ? $"{unit.ServiceDefinitionId}|{orderId}"
                    : $"{unit.ServiceDefinitionId}|{unit.TravellerId}|{string.Join(',', unit.CoveredFlightIds.Order())}")
                .ToList();

            Require(occurrences.Distinct(StringComparer.Ordinal).Count() == occurrences.Count, nameof(Units));

            var reservation = new AncillaryReservation(id, idempotencyKey, orderId, reference, requestedExpiresAt, now);

            reservation._units.AddRange(units.Select(unit => new AncillaryReservationUnit(idGenerator.NewId(), id, unit)));

            return reservation;
        }

        public void EnsureSameContent(
            long orderId,
            string reference,
            DateTimeOffset? requestedExpiresAt,
            IReadOnlyList<AncillaryReservationUnitArgs> units)
        {
            var same = orderId == OrderId
                       && reference == Reference
                       && requestedExpiresAt == RequestedExpiresAt
                       && units.Count == _units.Count
                       && _units.All(unit => units.Count(unit.IsHeldBy) == 1);

            if (!same)
                throw ExceptionFactory.AncillaryHoldIdempotencyKeyReused();
        }

        public AncillaryReservationUnitStatus StatusOf(AncillaryReservationUnit unit, DateTimeOffset now)
            => unit.Status == AncillaryReservationUnitStatus.Held && ExpiresAt is { } expiresAt && now >= expiresAt
                ? AncillaryReservationUnitStatus.Expired
                : unit.Status;

        public AncillaryReservationStatus EffectiveStatus(DateTimeOffset now)
            => Status == AncillaryReservationStatus.Held && ExpiresAt is { } expiresAt && now >= expiresAt
                ? AncillaryReservationStatus.Expired
                : Status;

        public bool Confirm(DateTimeOffset now)
        {
            var statuses = _units.Select(unit => StatusOf(unit, now)).ToList();

            if (statuses.All(status => status == AncillaryReservationUnitStatus.Held))
            {
                foreach (var unit in _units)
                    unit.ChangeStatus(AncillaryReservationUnitStatus.Confirmed);

                Status = AncillaryReservationStatus.Confirmed;
                UpdatedAt = now;

                return true;
            }

            if (statuses.All(status => status == AncillaryReservationUnitStatus.Confirmed))
                return false;

            if (statuses.Contains(AncillaryReservationUnitStatus.Expired))
                throw ExceptionFactory.AncillaryHoldHasExpired();

            throw ExceptionFactory.AncillaryHoldIsInMixedState();
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(field);
        }
    }
}
