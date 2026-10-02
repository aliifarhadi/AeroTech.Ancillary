using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.ServiceSubCodeAggregate
{
    public sealed class ServiceSubCodeRepository : IServiceSubCodeRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public ServiceSubCodeRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(ServiceSubCode subCode, CancellationToken cancellationToken = default)
            => await _dbContext.ServiceSubCodes.AddAsync(subCode, cancellationToken);

        public Task<ServiceSubCode?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.ServiceSubCodes.FirstOrDefaultAsync(subCode => subCode.Id == id, cancellationToken);

        public Task<ServiceSubCode?> FindAsync(int ownerAirlineId, string code, CancellationToken cancellationToken = default)
            => _dbContext.ServiceSubCodes
                .FirstOrDefaultAsync(subCode => subCode.OwnerAirlineId == ownerAirlineId && subCode.Code == code, cancellationToken);

        public Task<ServiceSubCode?> FindActiveAsync(int ownerAirlineId, string code, CancellationToken cancellationToken = default)
            => _dbContext.ServiceSubCodes
                .FirstOrDefaultAsync(subCode => subCode.OwnerAirlineId == ownerAirlineId
                                                && subCode.Code == code
                                                && subCode.Status == ServiceSubCodeStatus.Active,
                    cancellationToken);
    }
}
