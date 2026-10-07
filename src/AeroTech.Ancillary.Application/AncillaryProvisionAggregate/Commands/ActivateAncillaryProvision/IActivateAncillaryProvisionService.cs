using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision
{
    public interface IActivateAncillaryProvisionService
    {
        Task<ProvisionResult> ActivateAsync(IActivateAncillaryProvisionCommand command, CancellationToken cancellationToken = default);
    }
}
