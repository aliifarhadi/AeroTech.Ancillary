using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision
{
    public interface IChangeAncillaryProvisionService
    {
        Task<ProvisionResult> ChangeAsync(IChangeAncillaryProvisionCommand command, CancellationToken cancellationToken = default);
    }
}
