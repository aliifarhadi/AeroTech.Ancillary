using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAdvancePurchase.Backoffice
{
    public sealed record BackofficeChangeProvisionAdvancePurchaseCommand(
        long ProvisionId,
        ProvisionAdvancePurchaseInput? AdvancePurchase) : IRequest<ProvisionResult>, IChangeProvisionAdvancePurchaseCommand;
}
