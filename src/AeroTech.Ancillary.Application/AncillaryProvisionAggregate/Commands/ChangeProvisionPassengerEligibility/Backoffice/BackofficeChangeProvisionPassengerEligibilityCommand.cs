using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPassengerEligibility.Backoffice
{
    public sealed record BackofficeChangeProvisionPassengerEligibilityCommand(
        long ProvisionId,
        ProvisionPassengerEligibilityInput? PassengerEligibility) : IRequest<ProvisionResult>, IChangeProvisionPassengerEligibilityCommand;
}
