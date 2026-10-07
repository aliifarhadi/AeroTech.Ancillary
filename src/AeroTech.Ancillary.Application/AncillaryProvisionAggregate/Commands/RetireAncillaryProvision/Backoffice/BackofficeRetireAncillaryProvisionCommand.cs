using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision.Backoffice
{
    public sealed record BackofficeRetireAncillaryProvisionCommand(long ProvisionId)
        : IRequest<ProvisionResult>, IRetireAncillaryProvisionCommand;
}
