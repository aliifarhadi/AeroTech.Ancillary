using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBaggageApplication
{
    public interface IChangeProvisionBaggageApplicationCommand
    {
        long ProvisionId { get; }

        ProvisionBaggageApplicationInput? BaggageApplication { get; }
    }
}
