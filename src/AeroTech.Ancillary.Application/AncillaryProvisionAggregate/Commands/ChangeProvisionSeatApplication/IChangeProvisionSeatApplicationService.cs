using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeatApplication
{
    public interface IChangeProvisionSeatApplicationService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionSeatApplicationCommand command, CancellationToken cancellationToken = default);
    }
}
