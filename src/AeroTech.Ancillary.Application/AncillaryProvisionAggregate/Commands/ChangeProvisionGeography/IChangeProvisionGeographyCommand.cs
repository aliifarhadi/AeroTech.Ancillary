using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionGeography
{
    public interface IChangeProvisionGeographyCommand
    {
        long ProvisionId { get; }

        ProvisionGeographyInput? Geography { get; }
    }
}
