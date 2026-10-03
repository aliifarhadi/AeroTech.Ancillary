using AeroTech.Ancillary.Query.ServiceReservationAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.ServiceReservationAggregate.Queries.GetServiceReservationById.Service
{
    public sealed record ServiceGetServiceReservationByIdQuery(long ServiceReservationId) : IRequest<ServiceReservationDto>;
}
