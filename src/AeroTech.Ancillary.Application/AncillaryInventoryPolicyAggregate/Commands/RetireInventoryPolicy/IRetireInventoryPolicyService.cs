using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.RetireInventoryPolicy
{
    public interface IRetireInventoryPolicyService
    {
        Task<InventoryPolicyResult> RetireAsync(IRetireInventoryPolicyCommand command, CancellationToken cancellationToken = default);
    }
}
