using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductsPaginated
{
    public interface IGetAncillaryProductsPaginatedService
    {
        Task<GridData<AncillaryProductPaginatedRowDto>> ExecuteAsync(IAncillaryProductsPaginatedQuery query, CancellationToken cancellationToken = default);
    }
}
