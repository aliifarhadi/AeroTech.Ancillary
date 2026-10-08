using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFareApplication
{
    public interface IChangeProvisionFareApplicationCommand
    {
        long ProvisionId { get; }

        ProvisionFareApplicationInput? FareApplication { get; }
    }
}
