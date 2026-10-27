using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetOrderAncillaryOffers.OtaPanel
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class OtaPanelGetOrderAncillaryOffersQuery : IRequest<OrderAncillaryOffersDto>, IOrderAncillaryOffersQuery
    {
        public long? OrderId { get; set; }
    }
}
