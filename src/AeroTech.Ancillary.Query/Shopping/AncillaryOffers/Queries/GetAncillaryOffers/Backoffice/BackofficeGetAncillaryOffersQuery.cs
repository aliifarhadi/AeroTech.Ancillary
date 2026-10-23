using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class BackofficeGetAncillaryOffersQuery : IRequest<BackofficeAncillaryOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }
    }
}
