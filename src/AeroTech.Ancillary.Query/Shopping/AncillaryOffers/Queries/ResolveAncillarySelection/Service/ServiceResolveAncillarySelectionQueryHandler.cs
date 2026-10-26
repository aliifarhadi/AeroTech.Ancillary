using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveAncillarySelection.Service
{
    public sealed class ServiceResolveAncillarySelectionQueryHandler : IRequestHandler<ServiceResolveAncillarySelectionQuery, ServiceResolvedSelectionsDto>
    {
        private readonly IResolveAncillarySelectionService _service;

        public ServiceResolveAncillarySelectionQueryHandler(IResolveAncillarySelectionService service) => _service = service;

        public async Task<ServiceResolvedSelectionsDto> Handle(ServiceResolveAncillarySelectionQuery query, CancellationToken cancellationToken)
            => ServiceResolvedSelectionsMapper.Map(await _service.ExecuteAsync(query.SelectionId, cancellationToken));
    }
}
