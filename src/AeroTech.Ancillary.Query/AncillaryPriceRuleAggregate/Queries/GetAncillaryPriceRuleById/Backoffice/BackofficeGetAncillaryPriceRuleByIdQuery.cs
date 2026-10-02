using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRuleById.Backoffice
{
    public sealed record BackofficeGetAncillaryPriceRuleByIdQuery(long AncillaryPriceRuleId) : IRequest<BackofficeAncillaryPriceRuleDto>;
}
