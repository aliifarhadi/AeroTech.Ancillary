using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail.OtaPanel
{
    public sealed class OtaPanelGetGroupedOfferDetailQueryHandler : IRequestHandler<OtaPanelGetGroupedOfferDetailQuery, OtaPanelGroupedOfferDetailDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetGroupedOfferDetailService _service;

        public OtaPanelGetGroupedOfferDetailQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetGroupedOfferDetailService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OtaPanelGroupedOfferDetailDto> Handle(OtaPanelGetGroupedOfferDetailQuery query, CancellationToken cancellationToken)
            => OtaPanelGroupedOfferDetailMapper.Map(
                await _service.ExecuteAsync(
                    query,
                    query.Family,
                    query.Selection is null ? null : OtaPanelSelectionInputMapper.ToSelection(query.Selection),
                    await _salesScope.ResolveAsync(cancellationToken),
                    cancellationToken));
    }
}
