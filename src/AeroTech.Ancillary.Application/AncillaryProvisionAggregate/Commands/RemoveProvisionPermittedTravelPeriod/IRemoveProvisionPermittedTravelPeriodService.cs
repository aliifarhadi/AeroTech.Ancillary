using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionPermittedTravelPeriod
{
    public interface IRemoveProvisionPermittedTravelPeriodService
    {
        Task<ProvisionRuleRowResult> RemoveAsync(IRemoveProvisionPermittedTravelPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
