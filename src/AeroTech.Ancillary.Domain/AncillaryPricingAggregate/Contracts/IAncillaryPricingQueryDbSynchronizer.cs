using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts
{
    public interface IAncillaryPricingQueryDbSynchronizer : IQueryDbSynchronizer
    {
        Task ProjectAsync(AncillaryPricingReadModelSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
