using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeApplication
{
    public interface IChangeProvisionDayTimeApplicationCommand
    {
        long ProvisionId { get; }

        ProvisionDayTimeApplicationInput? DayTimeApplication { get; }
    }
}
