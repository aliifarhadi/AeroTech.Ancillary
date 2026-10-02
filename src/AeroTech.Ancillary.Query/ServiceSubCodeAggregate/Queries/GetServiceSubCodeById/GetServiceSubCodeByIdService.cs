using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Ancillary.Query._Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodeById
{
    public sealed class GetServiceSubCodeByIdService : IGetServiceSubCodeByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetServiceSubCodeByIdService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<BackofficeServiceSubCodeDto> ExecuteAsync(long serviceSubCodeId, CancellationToken cancellationToken = default)
        {
            var subCode = await _dbContext.ServiceSubCodes
                              .AsNoTracking()
                              .FirstOrDefaultAsync(row => row.Id == serviceSubCodeId, cancellationToken)
                          ?? throw ExceptionFactory.ServiceSubCodeNotFound();

            return new BackofficeServiceSubCodeDto(
                subCode.Id,
                subCode.OwnerAirlineId,
                subCode.Code,
                EnumValueDto.Of(subCode.Source),
                subCode.Rfic,
                subCode.GroupCode,
                subCode.SubGroupCode,
                subCode.Description1Code,
                subCode.Description2Code,
                subCode.CommercialName,
                EnumValueDto.Of(subCode.Status),
                subCode.CreatedAt);
        }
    }
}
