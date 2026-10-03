using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ConfirmServiceReservation
{
    public sealed class ConfirmServiceReservationService : IConfirmServiceReservationService
    {
        private readonly IServiceReservationRepository _reservations;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public ConfirmServiceReservationService(IServiceReservationRepository reservations, IUnitOfWork unitOfWork, IClock clock)
        {
            _reservations = reservations;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<ServiceReservationResult> ConfirmAsync(IConfirmServiceReservationCommand command, CancellationToken cancellationToken = default)
        {
            var reservation = await _reservations.GetAsync(command.ServiceReservationId, cancellationToken)
                              ?? throw ExceptionFactory.ServiceReservationNotFound();

            var now = _clock.GetDateTime();

            if (reservation.Confirm(now))
            {
                _reservations.Update(reservation);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return reservation.ToResult(now);
        }
    }
}
