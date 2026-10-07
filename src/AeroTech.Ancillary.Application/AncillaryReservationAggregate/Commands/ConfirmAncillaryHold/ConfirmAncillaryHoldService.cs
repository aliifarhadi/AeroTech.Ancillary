using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold
{
    public sealed class ConfirmAncillaryHoldService : IConfirmAncillaryHoldService
    {
        private readonly IAncillaryReservationRepository _reservations;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public ConfirmAncillaryHoldService(IAncillaryReservationRepository reservations, IUnitOfWork unitOfWork, IClock clock)
        {
            _reservations = reservations;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<AncillaryHoldResult> ConfirmAsync(IConfirmAncillaryHoldCommand command, CancellationToken cancellationToken = default)
        {
            var reservation = await _reservations.GetAsync(command.HoldId, cancellationToken)
                              ?? throw ExceptionFactory.AncillaryHoldNotFound();

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
