using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections.Service
{
    public sealed class ServiceResolveGroupedSelectionsQueryHandler : IRequestHandler<ServiceResolveGroupedSelectionsQuery, ServiceResolvedSelectionsDto>
    {
        private readonly IResolveGroupedSelectionsService _service;

        public ServiceResolveGroupedSelectionsQueryHandler(IResolveGroupedSelectionsService service) => _service = service;

        public async Task<ServiceResolvedSelectionsDto> Handle(ServiceResolveGroupedSelectionsQuery query, CancellationToken cancellationToken)
            => ServiceResolvedSelectionsMapper.Map(await _service.ExecuteAsync(query.Ids, cancellationToken));
    }
}
