using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetSeatMap.OtaPanel
{
    public sealed class OtaPanelGetSeatMapQuery : IRequest, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public string? OfferItemRef { get; set; }
    }
}
