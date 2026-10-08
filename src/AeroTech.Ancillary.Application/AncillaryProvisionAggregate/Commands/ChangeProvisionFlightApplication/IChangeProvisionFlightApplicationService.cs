using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFlightApplication
{
    public interface IChangeProvisionFlightApplicationService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionFlightApplicationCommand command, CancellationToken cancellationToken = default);
    }
}
