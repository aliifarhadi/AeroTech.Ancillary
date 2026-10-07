using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated
{
    public interface IGetAncillaryServiceDefinitionsPaginatedService
    {
        Task<GridData<ServiceDefinitionPaginatedRowDto>> ExecuteAsync(
            IAncillaryServiceDefinitionsPaginatedQuery query,
            CancellationToken cancellationToken = default);
    }
}
