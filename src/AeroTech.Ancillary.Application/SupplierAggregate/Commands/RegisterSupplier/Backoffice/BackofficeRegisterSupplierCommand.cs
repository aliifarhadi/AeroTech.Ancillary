using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier.Backoffice
{
    public sealed record BackofficeRegisterSupplierCommand(
        int OwnerAirlineId,
        string Name,
        SupplierFulfillmentKind FulfillmentKind,
        string? FulfillmentProviderKey) : IRequest<SupplierResult>, IRegisterSupplierCommand;
}
