using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel
{
    public sealed class OtaPanelGetAncillaryOffersQuery : IRequest<OtaPanelAncillaryOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }
    }
}
