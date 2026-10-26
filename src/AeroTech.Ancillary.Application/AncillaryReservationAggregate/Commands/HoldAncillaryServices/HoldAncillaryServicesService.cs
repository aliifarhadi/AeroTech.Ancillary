using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class HoldAncillaryServicesService : IHoldAncillaryServicesService
    {
        private readonly IAncillaryReservationRepository _reservations;
        private readonly IAncillaryHoldAllocator _allocator;
        private readonly IInventoryLedger _ledger;
        private readonly IAncillaryHoldLock _holdLock;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public HoldAncillaryServicesService(
            IAncillaryReservationRepository reservations,
            IAncillaryHoldAllocator allocator,
            IInventoryLedger ledger,
            IAncillaryHoldLock holdLock,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _reservations = reservations;
            _allocator = allocator;
            _ledger = ledger;
            _holdLock = holdLock;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public Task<AncillaryHoldResult> HoldAsync(IHoldAncillaryServicesCommand command, CancellationToken cancellationToken = default)
            => HoldAsync(
                new AncillaryHoldHeader(command.IdempotencyKey, command.OrderId, command.Reference, command.RequestedExpiresAt),
                command.Services
                    .Select(service => new AncillaryReservationUnitArgs(
                        service.OrderServiceId,
                        service.ServiceDefinitionId,
                        service.ProvisionId,
                        service.TravellerId,
                        service.CoverageScope,
                        service.CoveredFlightIds,
                        service.Quantity,
                        AppointmentTimeWithOffset: service.InventoryFacts?.TimeWithOffset,
                        GuestCount: service.InventoryFacts?.GuestCount))
                    .ToList(),
                cancellationToken);

        public async Task<AncillaryHoldResult> HoldAsync(
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            CancellationToken cancellationToken = default)
        {
            var now = _clock.GetDateTime();
            var stored = await _reservations.FindByIdempotencyKeyAsync(header.IdempotencyKey, cancellationToken);

            if (stored is not null)
                return Replayed(stored, header, units, now);

            await using var orderLock = await _holdLock.AcquireAsync(header.OrderId, cancellationToken);

            try
            {
                var allocated = await _allocator.AllocateAsync(units, cancellationToken);
                var reservation = await HoldAsync(header, allocated, now, cancellationToken);

                return reservation.ToResult(now);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                var winner = await _reservations.FindByIdempotencyKeyAsync(header.IdempotencyKey, cancellationToken);

                if (winner is null)
                    throw;

                return Replayed(winner, header, units, now);
            }
        }

        public async Task<AncillaryHoldResult> AppendSeatsAsync(
            long holdId,
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationUnitArgs> seats,
            CancellationToken cancellationToken = default)
        {
            var orderId = await _reservations.FindOrderIdAsync(holdId, cancellationToken)
                          ?? throw ExceptionFactory.AncillaryHoldNotFound();

            await using var orderLock = await _holdLock.AcquireAsync(orderId, cancellationToken);

            var reservation = await _reservations.GetAsync(holdId, cancellationToken)
                              ?? throw ExceptionFactory.AncillaryHoldNotFound();
            var now = _clock.GetDateTime();

            if (reservation.AppendSeats(header.IdempotencyKey, header.OrderId, header.Reference, header.RequestedExpiresAt, seats, _idGenerator, now))
            {
                var reserved = await _reservations.ReservedOrderServiceIdsAsync(
                    seats.Select(seat => seat.OrderServiceId).ToList(),
                    now,
                    cancellationToken);

                if (reserved.Count > 0)
                    throw ExceptionFactory.AncillaryHoldOrderServiceAlreadyReserved(reserved[0]);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return reservation.ToResult(now);
        }

        public async Task<AncillaryHoldResult?> FindHeldSeatsAsync(
            long? holdId,
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationSeatSelectionArgs> seats,
            CancellationToken cancellationToken = default)
        {
            var stored = holdId is { } id
                ? await _reservations.FindAsync(id, cancellationToken) ?? throw ExceptionFactory.AncillaryHoldNotFound()
                : await _reservations.FindByIdempotencyKeyAsync(header.IdempotencyKey, cancellationToken);
            var appendIdempotencyKey = holdId is null ? null : header.IdempotencyKey;

            return stored is not null && stored.HoldsSeats(appendIdempotencyKey, header.OrderId, header.Reference, header.RequestedExpiresAt, seats)
                ? stored.ToResult(_clock.GetDateTime())
                : null;
        }

        private async Task<AncillaryReservation> HoldAsync(
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var sources = units
                .SelectMany(unit => unit.Allocations ?? [])
                .Select(allocation => new InventorySourceKey(allocation.ResourceKind, allocation.InventorySourceId))
                .ToList();

            await using var ledger = await _ledger.OpenAsync(sources, cancellationToken);

            AncillaryHoldChecks.EnsureAvailable(units, await ledger.BalancesAsync(now, cancellationToken));

            var reserved = await _reservations.ReservedOrderServiceIdsAsync(
                units.Select(unit => unit.OrderServiceId).ToList(),
                now,
                cancellationToken);

            if (reserved.Count > 0)
                throw ExceptionFactory.AncillaryHoldOrderServiceAlreadyReserved(reserved[0]);

            var reservation = AncillaryReservation.Hold(
                _idGenerator.NewId(),
                header.IdempotencyKey,
                header.OrderId,
                header.Reference,
                header.RequestedExpiresAt,
                units,
                _idGenerator,
                now);

            await _reservations.AddAsync(reservation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await ledger.CommitAsync(cancellationToken);

            return reservation;
        }

        private static AncillaryHoldResult Replayed(
            AncillaryReservation stored,
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            DateTimeOffset now)
        {
            stored.EnsureSameContent(header.OrderId, header.Reference, header.RequestedExpiresAt, units);

            return stored.ToResult(now);
        }
    }
}
