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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public HoldAncillaryServicesService(
            IAncillaryReservationRepository reservations,
            IAncillaryServiceDefinitionRepository definitions,
            IAncillaryProvisionRepository provisions,
            ISupplierRepository suppliers,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _reservations = reservations;
            _definitions = definitions;
            _provisions = provisions;
            _suppliers = suppliers;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<AncillaryHoldResult> HoldAsync(IHoldAncillaryServicesCommand command, CancellationToken cancellationToken = default)
        {
            var now = _clock.GetDateTime();
            var units = command.Services
                .Select(service => new AncillaryReservationUnitArgs(
                    service.OrderServiceId,
                    service.ServiceDefinitionId,
                    service.ProvisionId,
                    service.TravellerId,
                    service.CoverageScope,
                    service.CoveredFlightIds,
                    service.Quantity))
                .ToList();
            var stored = await _reservations.FindByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);

            if (stored is not null)
                return Replayed(stored, command, units, now);

            try
            {
                var reservation = await NewHoldAsync(command, units, now, cancellationToken);

                await _reservations.AddAsync(reservation, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return reservation.ToResult(now);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                var winner = await _reservations.FindByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);

                if (winner is null)
                    throw;

                return Replayed(winner, command, units, now);
            }
        }

        private async Task<AncillaryReservation> NewHoldAsync(
            IHoldAncillaryServicesCommand command,
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
                command.IdempotencyKey,
                command.OrderId,
                command.Reference,
                command.RequestedExpiresAt,
                units,
                _idGenerator,
                now);
        }

        private static AncillaryHoldResult Replayed(
            AncillaryReservation stored,
            IHoldAncillaryServicesCommand command,
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            DateTimeOffset now)
        {
            stored.EnsureSameContent(command.OrderId, command.Reference, command.RequestedExpiresAt, units);

            return stored.ToResult(now);
        }
    }
}
