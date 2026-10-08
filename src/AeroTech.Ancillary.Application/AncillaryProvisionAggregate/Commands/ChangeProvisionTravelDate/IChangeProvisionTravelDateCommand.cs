using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate
{
    public interface IChangeProvisionTravelDateCommand
    {
        long ProvisionId { get; }

        ProvisionTravelDateInput? TravelDate { get; }
    }
}
