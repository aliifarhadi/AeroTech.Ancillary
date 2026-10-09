using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyByServiceIdentity
{
    public interface IGetInventoryPolicyByServiceIdentityService
    {
        Task<BackofficeInventoryPolicyDto> ExecuteAsync(string serviceDefinitionRef, CancellationToken cancellationToken = default);
    }
}
