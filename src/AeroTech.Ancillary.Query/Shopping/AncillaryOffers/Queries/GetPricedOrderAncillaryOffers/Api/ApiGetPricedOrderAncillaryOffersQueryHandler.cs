using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedOrderAncillaryOffers.Api
{
    public sealed class ApiGetPricedOrderAncillaryOffersQueryHandler : IRequestHandler<ApiGetPricedOrderAncillaryOffersQuery, OrderAncillaryOffersDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetPricedOrderAncillaryOffersService _service;

        public ApiGetPricedOrderAncillaryOffersQueryHandler(IApiSalesScopeResolver salesScope, IGetPricedOrderAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OrderAncillaryOffersDto> Handle(ApiGetPricedOrderAncillaryOffersQuery query, CancellationToken cancellationToken)
            => AncillaryOffersMapper.Map(await _service.ExecuteAsync(query.SelectionId, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
