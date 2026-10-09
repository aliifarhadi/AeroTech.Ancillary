using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAirportServiceRule.Backoffice
{
    public sealed record BackofficeChangeProvisionAirportServiceRuleCommand(
        long ProvisionId,
        ProvisionAirportServiceRuleInput? AirportServiceRule) : IRequest<ProvisionResult>, IChangeProvisionAirportServiceRuleCommand;
}
