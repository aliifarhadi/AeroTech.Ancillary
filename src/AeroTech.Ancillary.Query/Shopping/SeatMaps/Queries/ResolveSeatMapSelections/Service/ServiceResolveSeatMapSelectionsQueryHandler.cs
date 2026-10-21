using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections.Service
{
    public sealed class ServiceResolveSeatMapSelectionsQueryHandler : IRequestHandler<ServiceResolveSeatMapSelectionsQuery, ServiceSeatMapResolutionDto>
    {
        private readonly IResolveSeatMapSelectionsService _service;

        public ServiceResolveSeatMapSelectionsQueryHandler(IResolveSeatMapSelectionsService service) => _service = service;

        public async Task<ServiceSeatMapResolutionDto> Handle(ServiceResolveSeatMapSelectionsQuery query, CancellationToken cancellationToken)
            => ServiceSeatMapResolutionMapper.Map(await _service.ExecuteAsync(query.Ids, cancellationToken));
    }
}
