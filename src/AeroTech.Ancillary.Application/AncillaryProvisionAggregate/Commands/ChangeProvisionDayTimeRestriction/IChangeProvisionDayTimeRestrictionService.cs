using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeRestriction
{
    public interface IChangeProvisionDayTimeRestrictionService
    {
        Task<ProvisionConditionRowResult> ChangeAsync(IChangeProvisionDayTimeRestrictionCommand command, CancellationToken cancellationToken = default);
    }
}
