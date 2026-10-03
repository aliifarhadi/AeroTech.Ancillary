using AeroTech.Ancillary.Query.ServiceReservationAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.ServiceReservationAggregate.Queries.GetServiceReservationById.Service
{
    public sealed class ServiceGetServiceReservationByIdQueryHandler : IRequestHandler<ServiceGetServiceReservationByIdQuery, ServiceReservationDto>
    {
        private readonly IGetServiceReservationByIdService _service;

        public ServiceGetServiceReservationByIdQueryHandler(IGetServiceReservationByIdService service) => _service = service;

        public Task<ServiceReservationDto> Handle(ServiceGetServiceReservationByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceReservationId, cancellationToken);
    }
}
