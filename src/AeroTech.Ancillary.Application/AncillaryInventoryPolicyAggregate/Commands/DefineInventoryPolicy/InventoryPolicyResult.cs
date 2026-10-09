using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy
{
    public sealed record InventoryPolicyResult(
        long Id,
        int OwnerAirlineId,
        string ServiceDefinitionRef,
        long ServiceDefinitionId,
        InventoryAuthority Authority,
        LocalInventoryPattern? LocalPattern,
        string? ProviderKey,
        InventoryRecordStatus Status,
        long Version);
}
