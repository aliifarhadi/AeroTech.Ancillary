using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.ServiceSubCodeAggregate
{
    public sealed class ServiceSubCodeQueryDbSynchronizer : IServiceSubCodeQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public ServiceSubCodeQueryDbSynchronizer(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task ProjectAsync(ServiceSubCodeReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var subCode = await _dbContext.ServiceSubCodes.FirstOrDefaultAsync(row => row.Id == snapshot.ServiceSubCodeId, cancellationToken);

            if (subCode is null)
            {
                subCode = new ServiceSubCodeReadModel { Id = snapshot.ServiceSubCodeId };
                _dbContext.ServiceSubCodes.Add(subCode);
            }

            subCode.OwnerAirlineId = snapshot.OwnerAirlineId;
            subCode.Code = snapshot.Code;
            subCode.Source = snapshot.Source;
            subCode.Rfic = snapshot.Rfic;
            subCode.GroupCode = snapshot.GroupCode;
            subCode.SubGroupCode = snapshot.SubGroupCode;
            subCode.Description1Code = snapshot.Description1Code;
            subCode.Description2Code = snapshot.Description2Code;
            subCode.CommercialName = snapshot.CommercialName;
            subCode.Status = snapshot.Status;
            subCode.CreatedAt = snapshot.CreatedAt;
        }
    }
}
