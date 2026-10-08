using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeWindow
{
    public interface IChangeProvisionDayTimeWindowService
    {
        Task<ProvisionRuleRowResult> ChangeAsync(IChangeProvisionDayTimeWindowCommand command, CancellationToken cancellationToken = default);
    }
}
