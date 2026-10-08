using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBaggageApplication.Backoffice
{
    public sealed record BackofficeChangeProvisionBaggageApplicationCommand(
        long ProvisionId,
        ProvisionBaggageApplicationInput? BaggageApplication) : IRequest<ProvisionResult>, IChangeProvisionBaggageApplicationCommand;
}
