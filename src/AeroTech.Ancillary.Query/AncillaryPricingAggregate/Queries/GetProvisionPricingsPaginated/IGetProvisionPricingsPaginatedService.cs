using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetProvisionPricingsPaginated
{
    public interface IGetProvisionPricingsPaginatedService
    {
        Task<GridData<ProvisionPricingRowDto>> ExecuteAsync(IProvisionPricingsPaginatedQuery query, CancellationToken cancellationToken = default);
    }
}
