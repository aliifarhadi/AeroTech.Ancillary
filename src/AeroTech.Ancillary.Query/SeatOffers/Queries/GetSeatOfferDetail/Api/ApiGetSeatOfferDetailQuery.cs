using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.Api
{
    public sealed class ApiGetSeatOfferDetailQuery : IRequest<ApiSeatOfferDetailDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public string? OfferItemRef { get; set; }
    }
}
