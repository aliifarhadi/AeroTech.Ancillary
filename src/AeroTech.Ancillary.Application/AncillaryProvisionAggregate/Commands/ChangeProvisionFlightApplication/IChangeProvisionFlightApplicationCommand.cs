using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFlightApplication
{
    public interface IChangeProvisionFlightApplicationCommand
    {
        long ProvisionId { get; }

        ProvisionFlightApplicationInput? FlightApplication { get; }
    }
}
