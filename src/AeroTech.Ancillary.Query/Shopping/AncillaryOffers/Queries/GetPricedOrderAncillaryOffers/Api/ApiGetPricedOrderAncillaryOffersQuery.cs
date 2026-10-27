using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedOrderAncillaryOffers.Api
{
    public sealed record ApiGetPricedOrderAncillaryOffersQuery(string SelectionId) : IRequest<OrderAncillaryOffersDto>;
}
