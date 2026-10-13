using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.SeatOffers.Dto.OtaPanel;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.OtaPanel
{
    public sealed class OtaPanelGetSeatOffersQuery : IRequest<OtaPanelSeatOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }
    }
}
