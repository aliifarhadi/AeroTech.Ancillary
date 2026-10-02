using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodesPaginated
{
    public interface IGetServiceSubCodesPaginatedService
    {
        Task<GridData<ServiceSubCodePaginatedRowDto>> ExecuteAsync(IServiceSubCodesPaginatedQuery query, CancellationToken cancellationToken = default);
    }
}
