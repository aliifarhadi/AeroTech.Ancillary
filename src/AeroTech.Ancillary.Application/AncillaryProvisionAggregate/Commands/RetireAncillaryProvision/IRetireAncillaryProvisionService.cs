using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision
{
    public interface IRetireAncillaryProvisionService
    {
        Task<ProvisionResult> RetireAsync(IRetireAncillaryProvisionCommand command, CancellationToken cancellationToken = default);
    }
}
