using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public sealed class ApiGetAncillaryOffersQuery : IRequest<ApiAncillaryOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }
    }
}
