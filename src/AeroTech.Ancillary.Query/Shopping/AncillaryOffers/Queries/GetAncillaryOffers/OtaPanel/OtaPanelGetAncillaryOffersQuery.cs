using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel
{
    public sealed class OtaPanelGetAncillaryOffersQuery : IRequest<OtaPanelAncillaryOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }

        public long? OrderId { get; set; }
    }
}
