using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryConfigurationSnapshot
{
    public interface IGetInventoryConfigurationSnapshotService
    {
        Task<InventoryConfigurationSnapshotDto> ExecuteAsync(IGetInventoryConfigurationSnapshotQuery query, CancellationToken cancellationToken = default);
    }
}
