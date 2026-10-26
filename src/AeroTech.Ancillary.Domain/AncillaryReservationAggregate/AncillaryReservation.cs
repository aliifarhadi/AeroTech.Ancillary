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
            DateTimeOffset requestedExpiresAt,
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
            DateTimeOffset requestedExpiresAt,
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            IIdGenerator idGenerator,
            DateTimeOffset now)
        {
            Require(idempotencyKey is { Length: >= 1 and <= IdempotencyKeyMaxLength }, nameof(IdempotencyKey));
            Require(orderId > 0, nameof(OrderId));
            Require(reference is { Length: >= 1 and <= ReferenceMaxLength }, nameof(Reference));
            Require(requestedExpiresAt > now, nameof(RequestedExpiresAt));

            var reservation = new AncillaryReservation(id, idempotencyKey, orderId, reference, requestedExpiresAt, now);

            reservation.Add(units, null, idGenerator);

            return reservation;
        }

        public bool AppendSeats(
            string appendIdempotencyKey,
            long orderId,
            string reference,
            DateTimeOffset requestedExpiresAt,
            IReadOnlyList<AncillaryReservationUnitArgs> seats,
            IIdGenerator idGenerator,
            DateTimeOffset now)
        {
            Require(appendIdempotencyKey is { Length: >= 1 and <= IdempotencyKeyMaxLength }, nameof(AncillaryReservationUnit.AppendIdempotencyKey));
            Require(
                seats.All(seat => seat is { SeatNumber: not null, SeatMapSelectionId: not null, TravellerId: not null }),
                nameof(AncillaryReservationUnit.SeatNumber));

            var selections = seats
                .Select(seat => new AncillaryReservationSeatSelectionArgs(seat.OrderServiceId, seat.TravellerId!.Value, seat.SeatMapSelectionId!))
                .ToList();

            if (HoldsSeats(appendIdempotencyKey, orderId, reference, requestedExpiresAt, selections))
                return false;

            Require(orderId == OrderId, nameof(OrderId));
            Require(reference == Reference, nameof(Reference));
            Require(requestedExpiresAt == RequestedExpiresAt, nameof(RequestedExpiresAt));

            if (EffectiveStatus(now) != AncillaryReservationStatus.Held)
                throw NotTransitionable(now);

            Add(seats, appendIdempotencyKey, idGenerator);
            UpdatedAt = now;

            return true;
        }

        public bool HoldsSeats(
            string? appendIdempotencyKey,
            long orderId,
            string reference,
            DateTimeOffset requestedExpiresAt,
            IReadOnlyList<AncillaryReservationSeatSelectionArgs> seats)
        {
            var stored = _units.Where(unit => unit.AppendIdempotencyKey == appendIdempotencyKey).ToList();

            if (stored.Count == 0)
                return false;

            if (orderId != OrderId
                || reference != Reference
                || requestedExpiresAt != RequestedExpiresAt
                || seats.Count != stored.Count
                || !stored.All(unit => seats.Count(unit.IsSelectedBy) == 1))
                throw ExceptionFactory.AncillaryHoldIdempotencyKeyReused();

            return true;
        }

        public void EnsureSameContent(
            long orderId,
            string reference,
            DateTimeOffset requestedExpiresAt,
            IReadOnlyList<AncillaryReservationUnitArgs> units)
        {
            if (orderId != OrderId || reference != Reference || requestedExpiresAt != RequestedExpiresAt)
                throw ExceptionFactory.AncillaryHoldIdempotencyKeyReused();

            EnsureSameUnits(_units.Where(unit => unit.AppendIdempotencyKey is null).ToList(), units);
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
                    foreach (var unit in UnitsIn(AncillaryReservationUnitStatus.Held))
                        unit.Confirm();

                    Settle(now);
                    return true;
                default:
                    throw NotTransitionable(now);
            }
        }

        public bool Release(IReadOnlyCollection<long>? unitIds, DateTimeOffset now)
        {
            if (unitIds is null)
            {
                switch (EffectiveStatus(now))
                {
                    case AncillaryReservationStatus.Released:
                        return false;
                    case AncillaryReservationStatus.Held:
                        foreach (var unit in UnitsIn(AncillaryReservationUnitStatus.Held))
                            unit.Release();

                        Settle(now);
                        return true;
                    default:
                        throw NotTransitionable(now);
                }
            }

            var selected = Select(unitIds);

            if (selected.All(unit => unit.Status == AncillaryReservationUnitStatus.Released))
                return false;

            if (EffectiveStatus(now) != AncillaryReservationStatus.Held)
                throw NotTransitionable(now);

            EnsureEach(selected, unit => unit.Status is AncillaryReservationUnitStatus.Held or AncillaryReservationUnitStatus.Released);

            foreach (var unit in selected.Where(unit => unit.Status == AncillaryReservationUnitStatus.Held))
                unit.Release();

            Settle(now);
            return true;
        }

        public bool Cancel(string reasonCode, IReadOnlyCollection<long>? unitIds, DateTimeOffset now)
        {
            Require(reasonCode is { Length: >= 1 and <= CancellationReasonCodeMaxLength }, nameof(AncillaryReservationUnit.CancellationReasonCode));

            if (unitIds is null)
            {
                switch (EffectiveStatus(now))
                {
                    case AncillaryReservationStatus.Cancelled:
                        return false;
                    case AncillaryReservationStatus.Confirmed:
                        foreach (var unit in UnitsIn(AncillaryReservationUnitStatus.Confirmed))
                            unit.Cancel(reasonCode);

                        Settle(now);
                        return true;
                    default:
                        throw NotTransitionable(now);
                }
            }

            var selected = Select(unitIds);

            if (selected.Any(unit => unit.Status == AncillaryReservationUnitStatus.Cancelled && unit.CancellationReasonCode != reasonCode))
                throw ExceptionFactory.AncillaryHoldIsCancelled();

            if (selected.All(unit => unit.Status == AncillaryReservationUnitStatus.Cancelled))
                return false;

            if (EffectiveStatus(now) != AncillaryReservationStatus.Confirmed)
                throw NotTransitionable(now);

            EnsureEach(selected, unit => unit.Status is AncillaryReservationUnitStatus.Confirmed or AncillaryReservationUnitStatus.Cancelled);

            foreach (var unit in selected.Where(unit => unit.Status == AncillaryReservationUnitStatus.Confirmed))
                unit.Cancel(reasonCode);

            Settle(now);
            return true;
        }

        public bool Expire(DateTimeOffset now)
        {
            if (Status != AncillaryReservationStatus.Held || EffectiveStatus(now) != AncillaryReservationStatus.Expired)
                return false;

            foreach (var unit in UnitsIn(AncillaryReservationUnitStatus.Held))
                unit.Expire();

            Settle(now);

            return true;
        }

        private void Add(IReadOnlyList<AncillaryReservationUnitArgs> units, string? appendIdempotencyKey, IIdGenerator idGenerator)
        {
            var held = UnitsIn(AncillaryReservationUnitStatus.Held);

            Require(units.Count > 0, nameof(Units));
            Require(units.Select(unit => unit.OrderServiceId).Distinct().Count() == units.Count, nameof(AncillaryReservationUnit.OrderServiceId));

            var seats = held.Select(unit => unit.SeatKey)
                .Concat(units.Select(AncillaryReservationUnit.SeatKeyOf))
                .OfType<string>()
                .ToList();

            Require(seats.Distinct(StringComparer.Ordinal).Count() == seats.Count, nameof(AncillaryReservationUnit.SeatNumber));

            if (held.FirstOrDefault(unit => units.Any(added => added.OrderServiceId == unit.OrderServiceId)) is { } reserved)
                throw ExceptionFactory.AncillaryHoldOrderServiceAlreadyReserved(reserved.OrderServiceId);

            _units.AddRange(units.Select(unit => new AncillaryReservationUnit(idGenerator.NewId(), Id, unit, appendIdempotencyKey, idGenerator)));
        }

        private IReadOnlyList<AncillaryReservationUnit> Select(IReadOnlyCollection<long> unitIds)
        {
            Require(unitIds.Count > 0 && unitIds.All(unitId => unitId > 0) && unitIds.Distinct().Count() == unitIds.Count, nameof(Units));

            return unitIds
                .Select(unitId => _units.FirstOrDefault(unit => unit.Id == unitId) ?? throw ExceptionFactory.AncillaryHoldUnitNotFound(unitId))
                .ToList();
        }

        private List<AncillaryReservationUnit> UnitsIn(AncillaryReservationUnitStatus status)
            => _units.Where(unit => unit.Status == status).ToList();

        private void Settle(DateTimeOffset now)
        {
            var statuses = _units.Select(unit => unit.Status).ToHashSet();

            if (statuses.Contains(AncillaryReservationUnitStatus.Held) && statuses.Contains(AncillaryReservationUnitStatus.Confirmed))
                throw ExceptionFactory.AncillaryHoldIsInMixedState();

            Status = statuses.Contains(AncillaryReservationUnitStatus.Confirmed) ? AncillaryReservationStatus.Confirmed
                : statuses.Contains(AncillaryReservationUnitStatus.Held) ? AncillaryReservationStatus.Held
                : statuses.Contains(AncillaryReservationUnitStatus.Cancelled) ? AncillaryReservationStatus.Cancelled
                : statuses.Contains(AncillaryReservationUnitStatus.Expired) ? AncillaryReservationStatus.Expired
                : AncillaryReservationStatus.Released;
            UpdatedAt = now;
        }

        private static void EnsureSameUnits(IReadOnlyList<AncillaryReservationUnit> stored, IReadOnlyList<AncillaryReservationUnitArgs> requested)
        {
            if (requested.Count != stored.Count || !stored.All(unit => requested.Count(unit.IsHeldBy) == 1))
                throw ExceptionFactory.AncillaryHoldIdempotencyKeyReused();
        }

        private static void EnsureEach(IReadOnlyList<AncillaryReservationUnit> units, Func<AncillaryReservationUnit, bool> allowed)
        {
            if (units.FirstOrDefault(unit => !allowed(unit)) is { } refused)
                throw NotTransitionable(refused.Status);
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

        private static Exception NotTransitionable(AncillaryReservationUnitStatus status)
            => status switch
            {
                AncillaryReservationUnitStatus.Expired => ExceptionFactory.AncillaryHoldHasExpired(),
                AncillaryReservationUnitStatus.Released => ExceptionFactory.AncillaryHoldIsReleased(),
                AncillaryReservationUnitStatus.Cancelled => ExceptionFactory.AncillaryHoldIsCancelled(),
                AncillaryReservationUnitStatus.Confirmed => ExceptionFactory.AncillaryHoldIsConfirmed(),
                _ => ExceptionFactory.AncillaryHoldIsNotConfirmed()
            };

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(field);
        }
    }
}
