using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated
{
    public sealed class GetAncillaryServiceDefinitionsPaginatedService : IGetAncillaryServiceDefinitionsPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetAncillaryServiceDefinitionsPaginatedService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<GridData<ServiceDefinitionPaginatedRowDto>> ExecuteAsync(
            IAncillaryServiceDefinitionsPaginatedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            var variant = AncillaryVariant.Find(query.VariantCode);

            if (query.VariantCode is not null && (variant is null || (query.Profile is not null && variant.Profile != query.Profile)))
                throw ExceptionFactory.ServiceDefinitionIsInvalid(nameof(query.VariantCode));

            var rows = _dbContext.AncillaryServiceDefinitions
                .AsNoTracking()
                .Where(definition => (query.OwnerAirlineId == null || definition.OwnerAirlineId == query.OwnerAirlineId)
                                     && (query.SupplierId == null || definition.SupplierId == query.SupplierId)
                                     && (query.ServiceDefinitionRef == null || definition.ServiceDefinitionRef == query.ServiceDefinitionRef)
                                     && (query.ServiceSubCode == null || definition.ServiceSubCode == query.ServiceSubCode)
                                     && (query.ServiceTypeCode == null || definition.ServiceTypeCode == query.ServiceTypeCode)
                                     && (query.GroupCode == null || definition.GroupCode == query.GroupCode)
                                     && (query.Status == null || definition.Status == query.Status)
                                     && (query.Profile == null || definition.Profile == query.Profile)
                                     && (query.VariantCode == null || definition.VariantCode == query.VariantCode)
                                     && (query.Search == null
                                         || definition.ServiceDefinitionRef.Contains(query.Search)
                                         || definition.CommercialName.Contains(query.Search)));

            var totalCount = await rows.LongCountAsync(cancellationToken);

            var page = await rows
                .OrderBy(definition => definition.ServiceDefinitionRef)
                .ThenByDescending(definition => definition.Version)
                .ThenBy(definition => definition.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return GridData<ServiceDefinitionPaginatedRowDto>.Create(
                PaginatedList<ServiceDefinitionPaginatedRowDto>.Create(
                    page.Select(AncillaryServiceDefinitionMapper.ToPaginatedRow),
                    pageNumber,
                    pageSize,
                    totalCount));
        }
    }
}
