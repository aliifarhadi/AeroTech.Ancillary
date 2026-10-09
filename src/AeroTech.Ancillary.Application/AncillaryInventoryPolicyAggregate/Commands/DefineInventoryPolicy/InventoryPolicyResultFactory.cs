using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy
{
    internal static class InventoryPolicyResultFactory
    {
        public static InventoryPolicyResult ToResult(this AncillaryInventoryPolicy policy)
            => new(
                policy.Id,
                policy.OwnerAirlineId,
                policy.ServiceDefinitionRef,
                policy.ServiceDefinitionId,
                policy.Authority,
                policy.LocalPattern,
                policy.ProviderKey,
                policy.Status,
                policy.Version);
    }
}
