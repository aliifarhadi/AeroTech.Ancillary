using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSalesRestrictions
{
    public interface IChangeProvisionSalesRestrictionsCommand
    {
        long ProvisionId { get; }

        ProvisionSalesRestrictionsInput? SalesRestrictions { get; }
    }
}
