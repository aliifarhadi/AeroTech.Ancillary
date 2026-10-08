using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeWindow
{
    public interface IRemoveProvisionDayTimeWindowService
    {
        Task<ProvisionRuleRowResult> RemoveAsync(IRemoveProvisionDayTimeWindowCommand command, CancellationToken cancellationToken = default);
    }
}
