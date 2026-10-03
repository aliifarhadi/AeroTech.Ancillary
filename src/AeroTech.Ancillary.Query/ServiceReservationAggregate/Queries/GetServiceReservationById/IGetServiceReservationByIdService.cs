using AeroTech.Ancillary.Query.ServiceReservationAggregate.Dto;

namespace AeroTech.Ancillary.Query.ServiceReservationAggregate.Queries.GetServiceReservationById
{
    public interface IGetServiceReservationByIdService
    {
        Task<ServiceReservationDto> ExecuteAsync(long serviceReservationId, CancellationToken cancellationToken = default);
    }
}
