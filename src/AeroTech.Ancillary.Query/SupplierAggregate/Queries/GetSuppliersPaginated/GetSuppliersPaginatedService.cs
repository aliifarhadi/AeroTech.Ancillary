using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.SupplierAggregate.Dto;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSupplierById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated
{
    public sealed class GetSuppliersPaginatedService : IGetSuppliersPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetSuppliersPaginatedService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<GridData<SupplierPaginatedRowDto>> ExecuteAsync(
            ISuppliersPaginatedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var rows = _dbContext.Suppliers
                .AsNoTracking()
                .Where(supplier => (query.OwnerAirlineId == null || supplier.OwnerAirlineId == query.OwnerAirlineId)
                                   && (query.FulfillmentKind == null || supplier.FulfillmentKind == query.FulfillmentKind)
                                   && (query.Status == null || supplier.Status == query.Status)
                                   && (query.Search == null || supplier.Name.Contains(query.Search)));

            var totalCount = await rows.LongCountAsync(cancellationToken);

            var page = await rows
                .OrderBy(supplier => supplier.Name)
                .ThenBy(supplier => supplier.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return GridData<SupplierPaginatedRowDto>.Create(
                PaginatedList<SupplierPaginatedRowDto>.Create(page.Select(SupplierMapper.ToPaginatedRow), pageNumber, pageSize, totalCount));
        }
    }
}
