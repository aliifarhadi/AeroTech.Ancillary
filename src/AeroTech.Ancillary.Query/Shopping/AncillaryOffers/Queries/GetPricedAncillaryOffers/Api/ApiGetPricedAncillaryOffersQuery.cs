using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers.Api
{
    public sealed record ApiGetPricedAncillaryOffersQuery(string SelectionId) : IRequest<AncillaryOffersDto>;
}
