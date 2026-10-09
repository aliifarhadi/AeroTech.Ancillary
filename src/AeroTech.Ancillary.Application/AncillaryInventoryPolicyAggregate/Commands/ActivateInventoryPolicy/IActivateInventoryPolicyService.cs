using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ActivateInventoryPolicy
{
    public interface IActivateInventoryPolicyService
    {
        Task<InventoryPolicyResult> ActivateAsync(IActivateInventoryPolicyCommand command, CancellationToken cancellationToken = default);
    }
}
