using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts
{
    public interface IAncillaryProductQueryDbSynchronizer : IQueryDbSynchronizer
    {
        Task ProjectAsync(AncillaryProductReadModelSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
