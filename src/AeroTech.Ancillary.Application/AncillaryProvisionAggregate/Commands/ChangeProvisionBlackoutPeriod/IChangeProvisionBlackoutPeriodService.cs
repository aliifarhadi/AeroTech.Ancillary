using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod
{
    public interface IChangeProvisionBlackoutPeriodService
    {
        Task<ProvisionRuleRowResult> ChangeAsync(IChangeProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
