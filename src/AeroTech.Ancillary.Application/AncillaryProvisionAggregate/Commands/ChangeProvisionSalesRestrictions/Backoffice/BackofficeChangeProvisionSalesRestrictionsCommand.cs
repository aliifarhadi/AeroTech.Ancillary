using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSalesRestrictions.Backoffice
{
    public sealed record BackofficeChangeProvisionSalesRestrictionsCommand(
        long ProvisionId,
        ProvisionSalesRestrictionsInput? SalesRestrictions) : IRequest<ProvisionResult>, IChangeProvisionSalesRestrictionsCommand;
}
