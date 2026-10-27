using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetOrderAncillaryOffers.OtaPanel
{
    public sealed class OtaPanelGetOrderAncillaryOffersQueryHandler : IRequestHandler<OtaPanelGetOrderAncillaryOffersQuery, OrderAncillaryOffersDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetOrderAncillaryOffersService _service;

        public OtaPanelGetOrderAncillaryOffersQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetOrderAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OrderAncillaryOffersDto> Handle(OtaPanelGetOrderAncillaryOffersQuery query, CancellationToken cancellationToken)
            => AncillaryOffersMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
