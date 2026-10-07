using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById.Service
{
    public sealed class ServiceGetAncillaryHoldByIdQueryHandler : IRequestHandler<ServiceGetAncillaryHoldByIdQuery, AncillaryHoldDto>
    {
        private readonly IGetAncillaryHoldByIdService _service;

        public ServiceGetAncillaryHoldByIdQueryHandler(IGetAncillaryHoldByIdService service) => _service = service;

        public Task<AncillaryHoldDto> Handle(ServiceGetAncillaryHoldByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.HoldId, cancellationToken);
    }
}
