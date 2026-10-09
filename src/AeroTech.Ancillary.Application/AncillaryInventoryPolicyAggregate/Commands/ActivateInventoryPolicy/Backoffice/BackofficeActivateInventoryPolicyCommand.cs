using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ActivateInventoryPolicy.Backoffice
{
    public sealed record BackofficeActivateInventoryPolicyCommand(
        long PolicyId,
        long ExpectedVersion) : IRequest<InventoryPolicyResult>, IActivateInventoryPolicyCommand;
}
