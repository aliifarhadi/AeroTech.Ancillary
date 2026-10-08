using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionSeasonalPeriod
{
    public interface IAddProvisionSeasonalPeriodService
    {
        Task<ProvisionConditionRowResult> AddAsync(IAddProvisionSeasonalPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
