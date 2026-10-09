using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy
{
    public interface IChangeInventoryPolicyService
    {
        Task<InventoryPolicyResult> ChangeAsync(IChangeInventoryPolicyCommand command, CancellationToken cancellationToken = default);
    }
}
