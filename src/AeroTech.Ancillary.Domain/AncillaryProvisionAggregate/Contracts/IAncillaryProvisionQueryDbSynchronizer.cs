using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts
{
    public interface IAncillaryProvisionQueryDbSynchronizer : IQueryDbSynchronizer
    {
        Task ProjectAsync(AncillaryProvisionReadModelSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
