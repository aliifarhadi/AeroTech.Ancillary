using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.SuspendInventoryPolicy.Backoffice
{
    public sealed record BackofficeSuspendInventoryPolicyCommand(
        long PolicyId,
        long ExpectedVersion) : IRequest<InventoryPolicyResult>, ISuspendInventoryPolicyCommand;
}
