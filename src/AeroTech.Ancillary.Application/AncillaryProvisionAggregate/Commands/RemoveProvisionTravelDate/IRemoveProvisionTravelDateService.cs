using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionTravelDate
{
    public interface IRemoveProvisionTravelDateService
    {
        Task<ProvisionConditionRowResult> RemoveAsync(IRemoveProvisionTravelDateCommand command, CancellationToken cancellationToken = default);
    }
}
