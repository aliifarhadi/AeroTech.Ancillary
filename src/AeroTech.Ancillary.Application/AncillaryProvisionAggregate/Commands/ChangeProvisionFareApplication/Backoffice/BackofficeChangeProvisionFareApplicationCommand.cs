using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFareApplication.Backoffice
{
    public sealed record BackofficeChangeProvisionFareApplicationCommand(
        long ProvisionId,
        ProvisionFareApplicationInput? FareApplication) : IRequest<ProvisionResult>, IChangeProvisionFareApplicationCommand;
}
