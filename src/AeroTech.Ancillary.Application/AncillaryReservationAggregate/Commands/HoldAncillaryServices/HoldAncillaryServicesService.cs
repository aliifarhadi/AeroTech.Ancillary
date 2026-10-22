using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class HoldAncillaryServicesService : IHoldAncillaryServicesService
    {
        private readonly IAncillaryReservationRepository _reservations;
        private readonly IAncillaryServiceDefinitionRepository _definitions;
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly ISupplierRepository _suppliers;
        private readonly IAncillaryHoldLock _holdLock;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public HoldAncillaryServicesService(
            IAncillaryReservationRepository reservations,
            IAncillaryServiceDefinitionRepository definitions,
            IAncillaryProvisionRepository provisions,
            ISupplierRepository suppliers,
            IAncillaryHoldLock holdLock,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _reservations = reservations;
            _definitions = definitions;
            _provisions = provisions;
            _suppliers = suppliers;
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
                        service.SeatMapSelectionId,
                        service.SeatNumber,
                        service.PricingId))
                    .ToList(),
                cancellationToken);

        public async Task<AncillaryHoldResult> HoldAsync(AncillaryHoldHeader header, IReadOnlyList<AncillaryReservationUnitArgs> units, CancellationToken cancellationToken = default)
        {
            var now = _clock.GetDateTime();
            var stored = await _reservations.FindByIdempotencyKeyAsync(header.IdempotencyKey, cancellationToken);

            if (stored is not null)
                return Replayed(stored, header, units, now);

            await using var orderLock = await _holdLock.AcquireAsync(header.OrderId, cancellationToken);

            try
            {
                var reservation = await NewHoldAsync(header, units, now, cancellationToken);

                await _reservations.AddAsync(reservation, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        private async Task<AncillaryReservation> NewHoldAsync(
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            foreach (var unit in units)
            {
                var definition = await _definitions.GetAsync(unit.ServiceDefinitionId, cancellationToken);
                var provision = await _provisions.GetAsync(unit.ProvisionId, cancellationToken);
                var supplier = definition is null ? null : await _suppliers.GetAsync(definition.SupplierId, cancellationToken);

                AncillaryHoldChecks.Accept(unit, definition, provision, supplier);
            }

            var reserved = await _reservations.ReservedOrderServiceIdsAsync(
                units.Select(unit => unit.OrderServiceId).ToList(),
                now,
                cancellationToken);

            if (reserved.Count > 0)
                throw ExceptionFactory.AncillaryHoldOrderServiceAlreadyReserved(reserved[0]);

            return AncillaryReservation.Hold(
                _idGenerator.NewId(),
                header.IdempotencyKey,
                header.OrderId,
                header.Reference,
                header.RequestedExpiresAt,
                units,
                _idGenerator,
                now);
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
