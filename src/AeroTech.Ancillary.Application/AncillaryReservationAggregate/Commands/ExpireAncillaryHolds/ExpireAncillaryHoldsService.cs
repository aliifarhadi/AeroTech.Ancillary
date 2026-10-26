using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ExpireAncillaryHolds
{
    public sealed class ExpireAncillaryHoldsService : IExpireAncillaryHoldsService
    {
        private readonly IAncillaryReservationRepository _reservations;
        private readonly IAncillaryHoldLock _holdLock;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public ExpireAncillaryHoldsService(
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

        public Task<IReadOnlyList<long>> ListDueOrderIdsAsync(int batchSize, CancellationToken cancellationToken = default)
            => _reservations.ListOrderIdsWithExpiredHeldAsync(_clock.GetDateTime(), batchSize, cancellationToken);

        public async Task<int> ExpireAsync(long orderId, CancellationToken cancellationToken = default)
        {
            await using var orderLock = await _holdLock.AcquireAsync(orderId, cancellationToken);

            var now = _clock.GetDateTime();
            var expired = 0;

            foreach (var reservation in await _reservations.ListExpiredHeldAsync(orderId, now, cancellationToken))
            {
                if (!reservation.Expire(now))
                    continue;

                _reservations.Update(reservation);
                expired++;
            }

            if (expired > 0)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

            return expired;
        }
    }
}
