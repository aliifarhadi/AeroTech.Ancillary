using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionGeography.Backoffice
{
    public sealed record BackofficeChangeProvisionGeographyCommand(
        long ProvisionId,
        ProvisionGeographyInput? Geography) : IRequest<ProvisionResult>, IChangeProvisionGeographyCommand;
}
