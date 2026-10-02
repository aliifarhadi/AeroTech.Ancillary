using System.Globalization;
using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Ancillary.Query._Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodesPaginated
{
    public sealed class GetServiceSubCodesPaginatedService : IGetServiceSubCodesPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetServiceSubCodesPaginatedService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<GridData<ServiceSubCodePaginatedRowDto>> ExecuteAsync(
            IServiceSubCodesPaginatedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var rows = _dbContext.ServiceSubCodes
                .AsNoTracking()
                .Where(subCode => (query.OwnerAirlineId == null || subCode.OwnerAirlineId == query.OwnerAirlineId)
                                  && (query.Code == null || subCode.Code == query.Code)
                                  && (query.Source == null || subCode.Source == query.Source)
                                  && (query.Status == null || subCode.Status == query.Status));

            var totalCount = await rows.LongCountAsync(cancellationToken);

            var page = await rows
                .OrderBy(subCode => subCode.Code)
                .ThenBy(subCode => subCode.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var projected = page.Select(subCode => new ServiceSubCodePaginatedRowDto
            {
                Id = subCode.Id.ToString(CultureInfo.InvariantCulture),
                OwnerAirlineId = subCode.OwnerAirlineId,
                Code = subCode.Code,
                Source = EnumValueDto.Of(subCode.Source),
                Rfic = subCode.Rfic,
                GroupCode = subCode.GroupCode,
                SubGroupCode = subCode.SubGroupCode,
                Description1Code = subCode.Description1Code,
                Description2Code = subCode.Description2Code,
                CommercialName = subCode.CommercialName,
                Status = EnumValueDto.Of(subCode.Status),
                CreatedAt = subCode.CreatedAt
            });

            return GridData<ServiceSubCodePaginatedRowDto>.Create(
                PaginatedList<ServiceSubCodePaginatedRowDto>.Create(projected, pageNumber, pageSize, totalCount));
        }
    }
}
