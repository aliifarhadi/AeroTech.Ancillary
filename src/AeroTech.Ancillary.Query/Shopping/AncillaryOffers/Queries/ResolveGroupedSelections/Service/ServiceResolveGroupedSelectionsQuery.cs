using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections.Service
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class ServiceResolveGroupedSelectionsQuery : IRequest<ServiceResolvedSelectionsDto>
    {
        public IReadOnlyList<string>? Ids { get; set; }
    }
}
