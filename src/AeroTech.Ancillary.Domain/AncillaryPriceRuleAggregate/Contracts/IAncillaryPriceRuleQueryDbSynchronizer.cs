using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts
{
    public interface IAncillaryPriceRuleQueryDbSynchronizer : IQueryDbSynchronizer
    {
        Task ProjectAsync(AncillaryPriceRuleReadModelSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
