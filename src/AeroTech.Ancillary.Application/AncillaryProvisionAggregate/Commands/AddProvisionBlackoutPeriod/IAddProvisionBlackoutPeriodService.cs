using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod
{
    public interface IAddProvisionBlackoutPeriodService
    {
        Task<ProvisionRuleRowResult> AddAsync(IAddProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
