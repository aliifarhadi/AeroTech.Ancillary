using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.RetireInventoryPolicy.Backoffice
{
    public sealed record BackofficeRetireInventoryPolicyCommand(
        long PolicyId,
        long ExpectedVersion) : IRequest<InventoryPolicyResult>, IRetireInventoryPolicyCommand;
}
