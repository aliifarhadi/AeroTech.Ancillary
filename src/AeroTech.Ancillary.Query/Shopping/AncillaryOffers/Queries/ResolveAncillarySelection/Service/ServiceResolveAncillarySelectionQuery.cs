using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveAncillarySelection.Service
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class ServiceResolveAncillarySelectionQuery : IRequest<ServiceResolvedSelectionsDto>
    {
        public string? SelectionId { get; set; }
    }
}
