using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeWindow
{
    public interface IAddProvisionDayTimeWindowService
    {
        Task<ProvisionRuleRowResult> AddAsync(IAddProvisionDayTimeWindowCommand command, CancellationToken cancellationToken = default);
    }
}
