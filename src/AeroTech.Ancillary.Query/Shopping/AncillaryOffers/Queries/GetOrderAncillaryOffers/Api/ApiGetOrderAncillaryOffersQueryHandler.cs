using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetOrderAncillaryOffers.Api
{
    public sealed class ApiGetOrderAncillaryOffersQueryHandler : IRequestHandler<ApiGetOrderAncillaryOffersQuery, OrderAncillaryOffersDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetOrderAncillaryOffersService _service;

        public ApiGetOrderAncillaryOffersQueryHandler(IApiSalesScopeResolver salesScope, IGetOrderAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OrderAncillaryOffersDto> Handle(ApiGetOrderAncillaryOffersQuery query, CancellationToken cancellationToken)
            => AncillaryOffersMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
