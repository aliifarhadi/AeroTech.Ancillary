using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.SupplierAggregate.Dto;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated
{
    public interface IGetSuppliersPaginatedService
    {
        Task<GridData<SupplierPaginatedRowDto>> ExecuteAsync(
            ISuppliersPaginatedQuery query,
            CancellationToken cancellationToken = default);
    }
}
