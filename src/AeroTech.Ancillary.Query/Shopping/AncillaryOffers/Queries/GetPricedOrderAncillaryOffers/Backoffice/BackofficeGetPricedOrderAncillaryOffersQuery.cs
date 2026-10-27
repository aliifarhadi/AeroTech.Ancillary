using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedOrderAncillaryOffers.Backoffice
{
    public sealed record BackofficeGetPricedOrderAncillaryOffersQuery(string SelectionId) : IRequest<BackofficeOrderAncillaryOffersDto>;
}
