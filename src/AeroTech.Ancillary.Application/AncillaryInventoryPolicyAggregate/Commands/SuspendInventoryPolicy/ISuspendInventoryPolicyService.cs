using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.SuspendInventoryPolicy
{
    public interface ISuspendInventoryPolicyService
    {
        Task<InventoryPolicyResult> SuspendAsync(ISuspendInventoryPolicyCommand command, CancellationToken cancellationToken = default);
    }
}
