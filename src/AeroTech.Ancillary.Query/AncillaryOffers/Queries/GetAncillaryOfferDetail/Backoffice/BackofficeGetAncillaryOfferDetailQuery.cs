using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.Backoffice
{
    public sealed class BackofficeGetAncillaryOfferDetailQuery : IRequest<BackofficeAncillaryOfferDetailDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public string? OfferItemRef { get; set; }

        public BackofficeAncillaryOfferOptions? Options { get; set; }
    }
}
