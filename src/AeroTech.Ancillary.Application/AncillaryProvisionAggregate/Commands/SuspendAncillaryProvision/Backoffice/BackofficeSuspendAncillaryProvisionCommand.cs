using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision.Backoffice
{
    public sealed record BackofficeSuspendAncillaryProvisionCommand(long ProvisionId)
        : IRequest<ProvisionResult>, ISuspendAncillaryProvisionCommand;
}
