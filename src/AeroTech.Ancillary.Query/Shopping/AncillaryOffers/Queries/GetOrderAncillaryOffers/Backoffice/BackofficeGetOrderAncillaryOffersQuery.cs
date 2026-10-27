using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetOrderAncillaryOffers.Backoffice
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class BackofficeGetOrderAncillaryOffersQuery : IRequest<BackofficeOrderAncillaryOffersDto>, IOrderAncillaryOffersQuery
    {
        public long? OrderId { get; set; }
    }
}
