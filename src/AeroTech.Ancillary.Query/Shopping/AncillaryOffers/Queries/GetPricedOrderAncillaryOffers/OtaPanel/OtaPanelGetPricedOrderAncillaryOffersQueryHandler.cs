using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedOrderAncillaryOffers.OtaPanel
{
    public sealed class OtaPanelGetPricedOrderAncillaryOffersQueryHandler : IRequestHandler<OtaPanelGetPricedOrderAncillaryOffersQuery, OrderAncillaryOffersDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetPricedOrderAncillaryOffersService _service;

        public OtaPanelGetPricedOrderAncillaryOffersQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetPricedOrderAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OrderAncillaryOffersDto> Handle(OtaPanelGetPricedOrderAncillaryOffersQuery query, CancellationToken cancellationToken)
            => AncillaryOffersMapper.Map(await _service.ExecuteAsync(query.SelectionId, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
