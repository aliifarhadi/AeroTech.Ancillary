using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAdvancePurchase
{
    public interface IChangeProvisionAdvancePurchaseService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionAdvancePurchaseCommand command, CancellationToken cancellationToken = default);
    }
}
