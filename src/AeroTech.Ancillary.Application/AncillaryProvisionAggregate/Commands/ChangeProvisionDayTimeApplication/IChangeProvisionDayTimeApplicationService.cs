using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeApplication
{
    public interface IChangeProvisionDayTimeApplicationService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionDayTimeApplicationCommand command, CancellationToken cancellationToken = default);
    }
}
