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
        private const int CancellationReasonCodeMaxLength = 50;

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
                    ? $"{unit.ServiceDefinitionId}|{orderId}|{unit.SelectionId}"
                    : $"{unit.ServiceDefinitionId}|{unit.TravellerId}|{string.Join(',', unit.CoveredFlightIds.Order())}|{unit.SelectionId}")
                .ToList();

            Require(occurrences.Distinct(StringComparer.Ordinal).Count() == occurrences.Count, nameof(Units));

            var seats = units
                .Where(unit => unit.SeatNumber is not null)
                .Select(unit => $"{string.Join(',', unit.CoveredFlightIds.Order())}|{unit.SeatNumber!.Trim().ToUpperInvariant()}")
                .ToList();

            Require(seats.Distinct(StringComparer.Ordinal).Count() == seats.Count, nameof(AncillaryReservationUnit.SeatNumber));

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
            switch (EffectiveStatus(now))
            {
                case AncillaryReservationStatus.Confirmed:
                    return false;
                case AncillaryReservationStatus.Held:
                    Transition(AncillaryReservationStatus.Confirmed, AncillaryReservationUnitStatus.Confirmed, now);
                    return true;
                default:
                    throw NotTransitionable(now);
            }
        }

        public bool Release(DateTimeOffset now)
        {
            switch (EffectiveStatus(now))
            {
                case AncillaryReservationStatus.Released:
                    return false;
                case AncillaryReservationStatus.Held:
                    Transition(AncillaryReservationStatus.Released, AncillaryReservationUnitStatus.Released, now);
                    return true;
                default:
                    throw NotTransitionable(now);
            }
        }

        public bool Cancel(string reasonCode, DateTimeOffset now)
        {
            Require(reasonCode is { Length: >= 1 and <= CancellationReasonCodeMaxLength }, nameof(AncillaryReservationUnit.CancellationReasonCode));

            switch (EffectiveStatus(now))
            {
                case AncillaryReservationStatus.Cancelled:
                    return false;
                case AncillaryReservationStatus.Confirmed:
                    foreach (var unit in _units)
                        unit.Cancel(reasonCode);

                    Status = AncillaryReservationStatus.Cancelled;
                    UpdatedAt = now;
                    return true;
                default:
                    throw NotTransitionable(now);
            }
        }

        public bool Expire(DateTimeOffset now)
        {
            if (Status != AncillaryReservationStatus.Held || EffectiveStatus(now) != AncillaryReservationStatus.Expired)
                return false;

            Transition(AncillaryReservationStatus.Expired, AncillaryReservationUnitStatus.Expired, now);

            return true;
        }

        private void Transition(AncillaryReservationStatus status, AncillaryReservationUnitStatus unitStatus, DateTimeOffset now)
        {
            foreach (var unit in _units)
                unit.ChangeStatus(unitStatus);

            Status = status;
            UpdatedAt = now;
        }

        private Exception NotTransitionable(DateTimeOffset now)
            => EffectiveStatus(now) switch
            {
                AncillaryReservationStatus.Expired => ExceptionFactory.AncillaryHoldHasExpired(),
                AncillaryReservationStatus.Released => ExceptionFactory.AncillaryHoldIsReleased(),
                AncillaryReservationStatus.Cancelled => ExceptionFactory.AncillaryHoldIsCancelled(),
                AncillaryReservationStatus.Confirmed => ExceptionFactory.AncillaryHoldIsConfirmed(),
                _ => ExceptionFactory.AncillaryHoldIsNotConfirmed()
            };

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(field);
        }
    }
}
