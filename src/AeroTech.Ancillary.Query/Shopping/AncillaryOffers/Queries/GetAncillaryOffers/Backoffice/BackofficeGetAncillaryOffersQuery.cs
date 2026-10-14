using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
{
    public sealed class BackofficeGetAncillaryOffersQuery : IRequest<BackofficeAncillaryOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }

        public BackofficeAncillaryOfferOptions? Options { get; set; }
    }

}
