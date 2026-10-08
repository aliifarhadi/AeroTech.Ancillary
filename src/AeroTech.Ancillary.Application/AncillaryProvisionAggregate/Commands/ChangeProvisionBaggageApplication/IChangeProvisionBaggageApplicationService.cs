using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBaggageApplication
{
    public interface IChangeProvisionBaggageApplicationService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionBaggageApplicationCommand command, CancellationToken cancellationToken = default);
    }
}
