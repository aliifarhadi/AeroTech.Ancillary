using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPermittedTravelPeriod
{
    public interface IChangeProvisionPermittedTravelPeriodService
    {
        Task<ProvisionRuleRowResult> ChangeAsync(IChangeProvisionPermittedTravelPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
