using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections.Service
{
    public sealed class ServiceResolveSelectionsQuery : IRequest<ServiceSelectionResolutionDto>
    {
        public long? OrderId { get; set; }

        public int? ExpectedOrderCommercialVersion { get; set; }

        public IReadOnlyList<string>? PricedSelectionRefs { get; set; }

        public string? CorrelationId { get; set; }
    }
}
