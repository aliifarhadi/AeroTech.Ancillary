using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.Extensions.Options;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ExpireAncillaryHolds
{
    public sealed class ExpireAncillaryHoldsService : IExpireAncillaryHoldsService
    {
        private readonly IAncillaryReservationRepository _reservations;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;
        private readonly int _batchSize;

        public ExpireAncillaryHoldsService(
            IAncillaryReservationRepository reservations,
            IUnitOfWork unitOfWork,
            IClock clock,
            IOptions<AncillaryHoldOptions> options)
        {
            _reservations = reservations;
            _unitOfWork = unitOfWork;
            _clock = clock;
            _batchSize = options.Value.ExpiryBatchSize;
        }

        public async Task<int> ExpireAsync(CancellationToken cancellationToken = default)
        {
            var now = _clock.GetDateTime();
            var expired = 0;

            foreach (var reservation in await _reservations.ListExpiredHeldAsync(now, _batchSize, cancellationToken))
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
