using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail.OtaPanel
{
    public sealed class OtaPanelGetAncillaryOfferDetailQueryHandler : IRequestHandler<OtaPanelGetAncillaryOfferDetailQuery, OtaPanelAncillaryOfferDetailDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOfferDetailService _service;

        public OtaPanelGetAncillaryOfferDetailQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetAncillaryOfferDetailService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OtaPanelAncillaryOfferDetailDto> Handle(OtaPanelGetAncillaryOfferDetailQuery query, CancellationToken cancellationToken)
            => OtaPanelAncillaryOfferDetailMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), OfferFamilyFilters.Services, cancellationToken));
    }
}
