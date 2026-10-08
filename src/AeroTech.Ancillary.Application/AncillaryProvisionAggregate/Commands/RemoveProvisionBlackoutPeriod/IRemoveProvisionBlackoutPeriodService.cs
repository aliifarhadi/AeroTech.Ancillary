using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod
{
    public interface IRemoveProvisionBlackoutPeriodService
    {
        Task<ProvisionConditionRowResult> RemoveAsync(IRemoveProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
