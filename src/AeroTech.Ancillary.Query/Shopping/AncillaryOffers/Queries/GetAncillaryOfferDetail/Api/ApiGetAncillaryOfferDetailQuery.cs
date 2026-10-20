using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail.Api
{
    public sealed class ApiGetAncillaryOfferDetailQuery : IRequest<ApiAncillaryOfferDetailDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public long? OrderId { get; set; }

        public string? OfferItemRef { get; set; }
    }
}
