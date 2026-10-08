using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeatApplication
{
    public interface IChangeProvisionSeatApplicationCommand
    {
        long ProvisionId { get; }

        ProvisionSeatApplicationInput? SeatApplication { get; }
    }
}
