namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy
{
    public interface IDefineInventoryPolicyService
    {
        Task<InventoryPolicyResult> DefineAsync(IDefineInventoryPolicyCommand command, CancellationToken cancellationToken = default);
    }
}
