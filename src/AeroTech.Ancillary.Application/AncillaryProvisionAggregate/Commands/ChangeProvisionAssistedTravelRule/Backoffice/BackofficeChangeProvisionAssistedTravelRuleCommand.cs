using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAssistedTravelRule.Backoffice
{
    public sealed record BackofficeChangeProvisionAssistedTravelRuleCommand(
        long ProvisionId,
        ProvisionAssistedTravelRuleInput? AssistedTravelRule) : IRequest<ProvisionResult>, IChangeProvisionAssistedTravelRuleCommand;
}
