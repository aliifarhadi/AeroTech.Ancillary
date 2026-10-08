using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate
{
    public interface IChangeProvisionTravelDateService
    {
        Task<ProvisionConditionRowResult> ChangeAsync(IChangeProvisionTravelDateCommand command, CancellationToken cancellationToken = default);
    }
}
