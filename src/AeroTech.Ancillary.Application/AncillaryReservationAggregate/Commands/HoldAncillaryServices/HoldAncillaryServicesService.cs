using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
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
        private readonly IHoldSelectionVerifier _verifier;
        private readonly IAncillaryHoldAllocator _allocator;
        private readonly IInventoryLedger _ledger;
        private readonly IAncillaryHoldLock _holdLock;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public HoldAncillaryServicesService(
            IAncillaryReservationRepository reservations,
            IHoldSelectionVerifier verifier,
            IAncillaryHoldAllocator allocator,
            IInventoryLedger ledger,
            IAncillaryHoldLock holdLock,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _reservations = reservations;
            _verifier = verifier;
            _allocator = allocator;
            _ledger = ledger;
            _holdLock = holdLock;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<AncillaryHoldReceipt> HoldAsync(IHoldAncillaryServicesCommand command, CancellationToken cancellationToken = default)
        {
            var header = new AncillaryHoldHeader(command.IdempotencyKey, command.OrderId, command.Reference, command.RequestedExpiresAt);
            var selections = AncillaryHoldFamilies.Selections(command);
            var stored = await _reservations.FindByIdempotencyKeyAsync(header.IdempotencyKey, cancellationToken);

            if (stored is not null)
            {
                stored.EnsureSameContent(header.OrderId, header.Reference, header.RequestedExpiresAt, selections.Select(SelectionArgs).ToList());

                return stored.ToReceipt(_clock.GetDateTime());
            }

            var verified = await _verifier.VerifyAsync(new HoldSelectionVerification(selections), cancellationToken);

            return await HoldAsync(header, selections.Zip(verified, UnitArgs).ToList(), cancellationToken);
        }

        public async Task<AncillaryHoldReceipt> HoldAsync(
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

                return reservation.ToReceipt(now);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                var winner = await _reservations.FindByIdempotencyKeyAsync(header.IdempotencyKey, cancellationToken);

                if (winner is null)
                    throw;

                return Replayed(winner, header, units, now);
            }
        }

        public async Task<AncillaryHoldReceipt> AppendSeatsAsync(
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
            var described = await _allocator.AllocateAsync(seats, cancellationToken);
            var now = _clock.GetDateTime();

            if (reservation.AppendSeats(header.IdempotencyKey, header.OrderId, header.Reference, header.RequestedExpiresAt, described, _idGenerator, now))
            {
                var reserved = await _reservations.ReservedOrderServiceIdsAsync(
                    seats.Select(seat => seat.OrderServiceId).ToList(),
                    now,
                    cancellationToken);

                if (reserved.Count > 0)
                    throw ExceptionFactory.AncillaryHoldOrderServiceAlreadyReserved(reserved[0]);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return reservation.ToReceipt(now);
        }

        public async Task<AncillaryHoldReceipt?> FindHeldSeatsAsync(
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
                ? stored.ToReceipt(_clock.GetDateTime())
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

        private static AncillaryReservationServiceSelectionArgs SelectionArgs(HoldSelection selection)
            => new(
                selection.OrderServiceId,
                selection.ProvisionId,
                selection.TravellerId,
                selection.FlightIds,
                selection.Quantity,
                Chosen(selection.Options));

        private static AncillaryReservationUnitArgs UnitArgs(HoldSelection selection, VerifiedHoldSelection verified)
            => new(
                selection.OrderServiceId,
                verified.ServiceDefinitionId,
                selection.ProvisionId,
                selection.TravellerId,
                verified.CoverageScope,
                selection.FlightIds,
                verified.Quantity,
                Selection: verified.Selection);

        private static AncillaryReservationSelectionArgs? Chosen(ISelectionOptions? options)
            => options switch
            {
                BaggageSelectionOptions baggage when baggage is not { BagRef: null, MeasuredWeightKg: null, Dimensions: null, WeightKg: null } =>
                    new AncillaryReservationSelectionArgs(Baggage: new BaggageSelectionArgs(
                        baggage.BagRef,
                        baggage.MeasuredWeightKg,
                        baggage.Dimensions is { } size ? new SelectedDimensionsArgs(size.LengthCm, size.WidthCm, size.HeightCm) : null,
                        baggage.WeightKg)),
                AssistedTravelHoldSelectionOptions assistedTravel => new AncillaryReservationSelectionArgs(AssistedTravel: new AssistedTravelSelectionArgs(assistedTravel.AssistanceSsrCode)),
                AirportServiceSelectionOptions airportService => new AncillaryReservationSelectionArgs(AirportService: new AirportServiceSelectionArgs(airportService.TimeWithOffset, airportService.GuestCount)),
                ConnectivitySelectionOptions { DeviceCount: { } deviceCount } => new AncillaryReservationSelectionArgs(Connectivity: new ConnectivitySelectionArgs(deviceCount)),
                _ => null
            };

        private static AncillaryHoldReceipt Replayed(
            AncillaryReservation stored,
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            DateTimeOffset now)
        {
            stored.EnsureSameContent(header.OrderId, header.Reference, header.RequestedExpiresAt, units);

            return stored.ToReceipt(now);
        }
    }
}
