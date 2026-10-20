using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections.Service
{
    public sealed class ServiceResolveSelectionsQueryHandler : IRequestHandler<ServiceResolveSelectionsQuery, ServiceSelectionResolutionDto>
    {
        private readonly IResolveSelectionsService _service;

        public ServiceResolveSelectionsQueryHandler(IResolveSelectionsService service) => _service = service;

        public async Task<ServiceSelectionResolutionDto> Handle(ServiceResolveSelectionsQuery query, CancellationToken cancellationToken)
            => ServiceSelectionResolutionMapper.Map(
                await _service.ExecuteAsync(query.OrderId, query.ExpectedOrderCommercialVersion, query.PricedSelectionRefs, cancellationToken),
                query.CorrelationId);
    }
}
