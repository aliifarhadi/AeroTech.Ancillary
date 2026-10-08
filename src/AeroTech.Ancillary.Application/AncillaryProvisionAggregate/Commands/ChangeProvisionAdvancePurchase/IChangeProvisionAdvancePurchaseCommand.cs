using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAdvancePurchase
{
    public interface IChangeProvisionAdvancePurchaseCommand
    {
        long ProvisionId { get; }

        ProvisionAdvancePurchaseInput? AdvancePurchase { get; }
    }
}
