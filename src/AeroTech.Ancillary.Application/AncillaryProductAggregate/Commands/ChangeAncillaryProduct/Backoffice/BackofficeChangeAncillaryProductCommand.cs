using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct.Backoffice
{
    public sealed record BackofficeChangeAncillaryProductCommand(
        long AncillaryProductId,
        string Name,
        string? Description,
        AncillarySalesScope SalesScope,
        ProductQuantity Quantity,
        ProductDocument Document,
        ProductCodes Codes,
        ProductTerms Terms,
        AncillaryInventoryControl InventoryControl,
        ProductBaggage? Baggage,
        ProductLounge? Lounge) : IRequest<AncillaryProductResult>, IChangeAncillaryProductCommand;
}
