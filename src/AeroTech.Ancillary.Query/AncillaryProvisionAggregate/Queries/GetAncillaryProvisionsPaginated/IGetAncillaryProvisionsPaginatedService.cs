using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated
{
    public interface IGetAncillaryProvisionsPaginatedService
    {
        Task<GridData<ProvisionPaginatedRowDto>> ExecuteAsync(
            IAncillaryProvisionsPaginatedQuery query,
            CancellationToken cancellationToken = default);
    }
}
