using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.Api
{
    public sealed class ApiGetAncillaryOfferDetailQuery : IRequest<ApiAncillaryOfferDetailDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public string? OfferItemRef { get; set; }
    }
}
