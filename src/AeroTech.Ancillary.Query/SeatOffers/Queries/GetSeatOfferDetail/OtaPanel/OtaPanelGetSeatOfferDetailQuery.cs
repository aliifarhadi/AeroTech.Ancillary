using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.SeatOffers.Dto.OtaPanel;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.OtaPanel
{
    public sealed class OtaPanelGetSeatOfferDetailQuery : IRequest<OtaPanelSeatOfferDetailDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public string? OfferItemRef { get; set; }
    }
}
