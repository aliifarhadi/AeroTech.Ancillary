using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetSeatMap.Api
{
    public sealed class ApiGetSeatMapQueryHandler : IRequestHandler<ApiGetSeatMapQuery>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetSeatMapService _service;

        public ApiGetSeatMapQueryHandler(IApiSalesScopeResolver salesScope, IGetSeatMapService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task Handle(ApiGetSeatMapQuery query, CancellationToken cancellationToken)
            => await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken);
    }
}
