using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public sealed class ApiGetAncillaryOffersQuery : IRequest<ApiAncillaryOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }
    }
}
