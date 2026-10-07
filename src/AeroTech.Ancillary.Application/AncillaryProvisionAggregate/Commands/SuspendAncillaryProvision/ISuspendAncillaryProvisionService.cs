using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision
{
    public interface ISuspendAncillaryProvisionService
    {
        Task<ProvisionResult> SuspendAsync(ISuspendAncillaryProvisionCommand command, CancellationToken cancellationToken = default);
    }
}
