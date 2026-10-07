using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision
{
    public interface IReactivateAncillaryProvisionService
    {
        Task<ProvisionResult> ReactivateAsync(IReactivateAncillaryProvisionCommand command, CancellationToken cancellationToken = default);
    }
}
