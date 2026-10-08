using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionSeasonalPeriod
{
    public interface IRemoveProvisionSeasonalPeriodService
    {
        Task<ProvisionConditionRowResult> RemoveAsync(IRemoveProvisionSeasonalPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
