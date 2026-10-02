using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductsPaginated
{
    public sealed class GetAncillaryProductsPaginatedService : IGetAncillaryProductsPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetAncillaryProductsPaginatedService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<GridData<AncillaryProductPaginatedRowDto>> ExecuteAsync(
            IAncillaryProductsPaginatedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var rows = _dbContext.AncillaryProducts
                .AsNoTracking()
                .Where(product => (query.OwnerAirlineId == null || product.OwnerAirlineId == query.OwnerAirlineId)
                                  && (query.ProductRef == null || product.ProductRef == query.ProductRef)
                                  && (query.Type == null || product.Type == query.Type)
                                  && (query.Status == null || product.Status == query.Status));

            var totalCount = await rows.LongCountAsync(cancellationToken);

            var page = await rows
                .OrderBy(product => product.ProductRef)
                .ThenByDescending(product => product.Version)
                .ThenBy(product => product.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var projected = page.Select(AncillaryProductMapper.ToPaginatedRow);

            return GridData<AncillaryProductPaginatedRowDto>.Create(
                PaginatedList<AncillaryProductPaginatedRowDto>.Create(projected, pageNumber, pageSize, totalCount));
        }
    }
}
