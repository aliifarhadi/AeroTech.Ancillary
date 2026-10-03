using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.ServiceReservationAggregate.Dto;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Query.ServiceReservationAggregate.Queries.GetServiceReservationById
{
    public sealed class GetServiceReservationByIdService : IGetServiceReservationByIdService
    {
        private readonly IServiceReservationRepository _reservations;
        private readonly IClock _clock;

        public GetServiceReservationByIdService(IServiceReservationRepository reservations, IClock clock)
        {
            _reservations = reservations;
            _clock = clock;
        }

        public async Task<ServiceReservationDto> ExecuteAsync(long serviceReservationId, CancellationToken cancellationToken = default)
        {
            var reservation = await _reservations.GetAsync(serviceReservationId, cancellationToken)
                              ?? throw ExceptionFactory.ServiceReservationNotFound();

            return ServiceReservationMapper.ToServiceReservation(reservation, _clock.GetDateTime());
        }
    }
}
