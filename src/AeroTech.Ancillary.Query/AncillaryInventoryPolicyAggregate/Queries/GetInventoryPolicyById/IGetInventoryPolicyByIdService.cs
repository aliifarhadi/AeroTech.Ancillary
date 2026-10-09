using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyById
{
    public interface IGetInventoryPolicyByIdService
    {
        Task<BackofficeInventoryPolicyDto> ExecuteAsync(long policyId, CancellationToken cancellationToken = default);
    }
}
