using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate
{
    public interface IChangeProvisionTravelDateService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionTravelDateCommand command, CancellationToken cancellationToken = default);
    }
}
