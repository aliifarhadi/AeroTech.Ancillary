using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRulesPaginated
{
    public interface IGetAncillaryPriceRulesPaginatedService
    {
        Task<GridData<AncillaryPriceRulePaginatedRowDto>> ExecuteAsync(IAncillaryPriceRulesPaginatedQuery query, CancellationToken cancellationToken = default);
    }
}
