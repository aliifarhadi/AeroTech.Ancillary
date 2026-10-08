using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeRestriction
{
    public interface IRemoveProvisionDayTimeRestrictionService
    {
        Task<ProvisionConditionRowResult> RemoveAsync(IRemoveProvisionDayTimeRestrictionCommand command, CancellationToken cancellationToken = default);
    }
}
