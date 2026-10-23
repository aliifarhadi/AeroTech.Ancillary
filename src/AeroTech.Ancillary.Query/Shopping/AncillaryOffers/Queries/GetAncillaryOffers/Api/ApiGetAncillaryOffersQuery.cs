using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class ApiGetAncillaryOffersQuery : IRequest<AncillaryOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }
    }
}
