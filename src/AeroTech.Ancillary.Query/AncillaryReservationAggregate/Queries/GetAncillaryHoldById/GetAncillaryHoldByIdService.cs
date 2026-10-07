using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById
{
    public sealed class GetAncillaryHoldByIdService : IGetAncillaryHoldByIdService
    {
        private readonly IAncillaryReservationRepository _reservations;
        private readonly IClock _clock;

        public GetAncillaryHoldByIdService(IAncillaryReservationRepository reservations, IClock clock)
        {
            _reservations = reservations;
            _clock = clock;
        }

        public async Task<AncillaryHoldDto> ExecuteAsync(long holdId, CancellationToken cancellationToken = default)
        {
            var reservation = await _reservations.GetAsync(holdId, cancellationToken)
                              ?? throw ExceptionFactory.AncillaryHoldNotFound();

            return AncillaryHoldMapper.ToAncillaryHold(reservation, _clock.GetDateTime());
        }
    }
}
