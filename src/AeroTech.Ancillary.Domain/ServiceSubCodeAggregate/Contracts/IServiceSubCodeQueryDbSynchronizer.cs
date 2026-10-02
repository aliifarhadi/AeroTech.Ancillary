using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts
{
    public interface IServiceSubCodeQueryDbSynchronizer : IQueryDbSynchronizer
    {
        Task ProjectAsync(ServiceSubCodeReadModelSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
