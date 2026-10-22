using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.CancelAncillaryHold
{
    public sealed class CancelAncillaryHoldService : ICancelAncillaryHoldService
    {
        private readonly IAncillaryReservationRepository _reservations;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public CancelAncillaryHoldService(IAncillaryReservationRepository reservations, IUnitOfWork unitOfWork, IClock clock)
        {
            _reservations = reservations;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<AncillaryHoldResult> CancelAsync(ICancelAncillaryHoldCommand command, CancellationToken cancellationToken = default)
        {
            var reservation = await _reservations.GetAsync(command.HoldId, cancellationToken)
                              ?? throw ExceptionFactory.AncillaryHoldNotFound();

            var now = _clock.GetDateTime();

            if (reservation.Cancel(command.ReasonCode, now))
            {
                _reservations.Update(reservation);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return reservation.ToResult(now);
        }
    }
}
