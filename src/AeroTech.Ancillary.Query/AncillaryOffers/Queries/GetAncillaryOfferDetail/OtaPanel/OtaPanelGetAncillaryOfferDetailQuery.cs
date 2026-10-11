using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.OtaPanel
{
    public sealed class OtaPanelGetAncillaryOfferDetailQuery : IRequest<OtaPanelAncillaryOfferDetailDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public string? OfferItemRef { get; set; }
    }
}
