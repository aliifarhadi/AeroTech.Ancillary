using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFareApplication
{
    public interface IChangeProvisionFareApplicationService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionFareApplicationCommand command, CancellationToken cancellationToken = default);
    }
}
