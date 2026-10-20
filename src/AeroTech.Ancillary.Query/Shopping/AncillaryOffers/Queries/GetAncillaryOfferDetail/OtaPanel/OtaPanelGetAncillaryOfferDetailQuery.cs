using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail.OtaPanel
{
    public sealed class OtaPanelGetAncillaryOfferDetailQuery : IRequest<OtaPanelAncillaryOfferDetailDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public long? OrderId { get; set; }

        public string? OfferItemRef { get; set; }
    }
}
