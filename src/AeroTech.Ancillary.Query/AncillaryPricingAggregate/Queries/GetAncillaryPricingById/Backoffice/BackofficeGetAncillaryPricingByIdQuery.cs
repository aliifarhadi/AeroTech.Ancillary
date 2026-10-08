using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById.Backoffice
{
    public sealed record BackofficeGetAncillaryPricingByIdQuery(long PricingId) : IRequest<BackofficePricingDto>;
}
