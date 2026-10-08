using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod
{
    public interface IChangeProvisionBlackoutPeriodService
    {
        Task<ProvisionConditionRowResult> ChangeAsync(IChangeProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
