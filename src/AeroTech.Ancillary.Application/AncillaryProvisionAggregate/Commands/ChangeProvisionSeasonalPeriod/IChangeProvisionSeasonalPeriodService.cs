using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeasonalPeriod
{
    public interface IChangeProvisionSeasonalPeriodService
    {
        Task<ProvisionConditionRowResult> ChangeAsync(IChangeProvisionSeasonalPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
