using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod
{
    public interface IRemoveProvisionBlackoutPeriodService
    {
        Task<ProvisionRuleRowResult> RemoveAsync(IRemoveProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
