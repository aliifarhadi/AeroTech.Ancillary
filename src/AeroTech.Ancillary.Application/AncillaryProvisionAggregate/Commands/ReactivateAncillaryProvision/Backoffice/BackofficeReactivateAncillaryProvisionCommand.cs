using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision.Backoffice
{
    public sealed record BackofficeReactivateAncillaryProvisionCommand(long ProvisionId)
        : IRequest<ProvisionResult>, IReactivateAncillaryProvisionCommand;
}
