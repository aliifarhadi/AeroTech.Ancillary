using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.CancelAncillaryHold
{
    public sealed class CancelAncillaryHoldService : ICancelAncillaryHoldService
    {
        private readonly IAncillaryReservationRepository _reservations;
        private readonly IAncillaryHoldLock _holdLock;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public CancelAncillaryHoldService(
            IAncillaryReservationRepository reservations,
            IAncillaryHoldLock holdLock,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _reservations = reservations;
            _holdLock = holdLock;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<AncillaryHoldReceipt> CancelAsync(ICancelAncillaryHoldCommand command, CancellationToken cancellationToken = default)
        {
            var orderId = await _reservations.FindOrderIdAsync(command.HoldId, cancellationToken)
                          ?? throw ExceptionFactory.AncillaryHoldNotFound();

            await using var orderLock = await _holdLock.AcquireAsync(orderId, cancellationToken);

            var reservation = await _reservations.GetAsync(command.HoldId, cancellationToken)
                              ?? throw ExceptionFactory.AncillaryHoldNotFound();

            var now = _clock.GetDateTime();

            if (reservation.Cancel(command.ReasonCode, command.UnitIds, now))
            {
                _reservations.Update(reservation);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return reservation.ToReceipt(now);
        }
    }
}
