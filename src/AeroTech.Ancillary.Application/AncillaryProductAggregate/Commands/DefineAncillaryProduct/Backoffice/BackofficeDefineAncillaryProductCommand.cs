using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct.Backoffice
{
    public sealed record BackofficeDefineAncillaryProductCommand(
        int OwnerAirlineId,
        string ProductRef,
        AncillaryProductType Type,
        string Name,
        string? Description,
        AncillarySalesScope SalesScope,
        ProductQuantity Quantity,
        ProductDocument Document,
        ProductCodes Codes,
        ProductTerms Terms,
        AncillaryInventoryControl InventoryControl,
        ProductBaggage? Baggage,
        ProductLounge? Lounge) : IRequest<AncillaryProductResult>, IDefineAncillaryProductCommand;
}
