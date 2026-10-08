using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPassengerEligibility
{
    public interface IChangeProvisionPassengerEligibilityService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionPassengerEligibilityCommand command, CancellationToken cancellationToken = default);
    }
}
