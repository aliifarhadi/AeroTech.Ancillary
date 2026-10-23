using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class OtaPanelGetAncillaryOffersQuery : IRequest<AncillaryOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }
    }
}
