using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedOrderAncillaryOffers.OtaPanel
{
    public sealed record OtaPanelGetPricedOrderAncillaryOffersQuery(string SelectionId) : IRequest<OrderAncillaryOffersDto>;
}
