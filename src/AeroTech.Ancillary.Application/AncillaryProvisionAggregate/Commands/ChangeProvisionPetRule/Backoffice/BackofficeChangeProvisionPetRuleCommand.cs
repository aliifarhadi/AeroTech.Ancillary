using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPetRule.Backoffice
{
    public sealed record BackofficeChangeProvisionPetRuleCommand(
        long ProvisionId,
        ProvisionPetRuleInput? PetRule) : IRequest<ProvisionResult>, IChangeProvisionPetRuleCommand;
}
