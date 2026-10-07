using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts
{
    public interface IAncillaryServiceDefinitionQueryDbSynchronizer : IQueryDbSynchronizer
    {
        Task ProjectAsync(AncillaryServiceDefinitionReadModelSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
