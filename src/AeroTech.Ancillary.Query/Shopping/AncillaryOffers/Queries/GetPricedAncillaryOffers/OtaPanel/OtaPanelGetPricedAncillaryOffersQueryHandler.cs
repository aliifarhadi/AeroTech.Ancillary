using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers.OtaPanel
{
    public sealed class OtaPanelGetPricedAncillaryOffersQueryHandler : IRequestHandler<OtaPanelGetPricedAncillaryOffersQuery, AncillaryOffersDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetPricedAncillaryOffersService _service;

        public OtaPanelGetPricedAncillaryOffersQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetPricedAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<AncillaryOffersDto> Handle(OtaPanelGetPricedAncillaryOffersQuery query, CancellationToken cancellationToken)
            => AncillaryOffersMapper.Map(await _service.ExecuteAsync(query.SelectionId, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
