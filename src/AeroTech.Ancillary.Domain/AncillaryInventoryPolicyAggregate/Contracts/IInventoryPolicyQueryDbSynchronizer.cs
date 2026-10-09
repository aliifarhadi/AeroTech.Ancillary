using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts
{
    public interface IInventoryPolicyQueryDbSynchronizer : IQueryDbSynchronizer
    {
        Task ProjectAsync(InventoryPolicyReadModelSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
