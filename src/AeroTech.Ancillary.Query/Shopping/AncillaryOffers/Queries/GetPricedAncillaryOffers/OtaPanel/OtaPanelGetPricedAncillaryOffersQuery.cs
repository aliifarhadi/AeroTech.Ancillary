using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers.OtaPanel
{
    public sealed record OtaPanelGetPricedAncillaryOffersQuery(string SelectionId) : IRequest<AncillaryOffersDto>;
}
