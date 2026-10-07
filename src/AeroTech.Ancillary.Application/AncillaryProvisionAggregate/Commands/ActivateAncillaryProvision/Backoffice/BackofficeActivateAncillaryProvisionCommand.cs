using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision.Backoffice
{
    public sealed record BackofficeActivateAncillaryProvisionCommand(long ProvisionId)
        : IRequest<ProvisionResult>, IActivateAncillaryProvisionCommand;
}
