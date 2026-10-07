using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.SupplierAggregate.Contracts
{
    public interface ISupplierQueryDbSynchronizer : IQueryDbSynchronizer
    {
        Task ProjectAsync(SupplierReadModelSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
