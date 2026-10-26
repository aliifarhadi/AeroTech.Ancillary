using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers.Backoffice
{
    public sealed record BackofficeGetPricedAncillaryOffersQuery(string SelectionId) : IRequest<BackofficeAncillaryOffersDto>;
}
