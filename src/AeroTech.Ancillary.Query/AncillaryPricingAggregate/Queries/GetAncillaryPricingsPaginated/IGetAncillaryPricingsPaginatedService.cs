using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated
{
    public interface IGetAncillaryPricingsPaginatedService
    {
        Task<GridData<PricingPaginatedRowDto>> ExecuteAsync(
            IAncillaryPricingsPaginatedQuery query,
            CancellationToken cancellationToken = default);
    }
}
