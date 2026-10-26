using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers.Api
{
    public sealed class ApiGetPricedAncillaryOffersQueryHandler : IRequestHandler<ApiGetPricedAncillaryOffersQuery, AncillaryOffersDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetPricedAncillaryOffersService _service;

        public ApiGetPricedAncillaryOffersQueryHandler(IApiSalesScopeResolver salesScope, IGetPricedAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<AncillaryOffersDto> Handle(ApiGetPricedAncillaryOffersQuery query, CancellationToken cancellationToken)
            => AncillaryOffersMapper.Map(await _service.ExecuteAsync(query.SelectionId, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
