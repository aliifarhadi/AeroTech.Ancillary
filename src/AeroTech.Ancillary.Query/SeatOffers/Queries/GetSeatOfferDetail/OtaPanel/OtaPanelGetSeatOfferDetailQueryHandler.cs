using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.OtaPanel;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.OtaPanel
{
    public sealed class OtaPanelGetSeatOfferDetailQueryHandler : IRequestHandler<OtaPanelGetSeatOfferDetailQuery, OtaPanelSeatOfferDetailDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOfferDetailService _service;

        public OtaPanelGetSeatOfferDetailQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetAncillaryOfferDetailService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OtaPanelSeatOfferDetailDto> Handle(OtaPanelGetSeatOfferDetailQuery query, CancellationToken cancellationToken)
        {
            var detail = await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), OfferFamilyFilters.Seat, cancellationToken);

            return OtaPanelSeatOffersMapper.Detail(detail);
        }
    }
}
