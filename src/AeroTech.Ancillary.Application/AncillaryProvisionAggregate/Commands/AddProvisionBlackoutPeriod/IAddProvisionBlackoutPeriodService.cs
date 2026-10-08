using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod
{
    public interface IAddProvisionBlackoutPeriodService
    {
        Task<ProvisionConditionRowResult> AddAsync(IAddProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
