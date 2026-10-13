using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.OtaPanel
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
