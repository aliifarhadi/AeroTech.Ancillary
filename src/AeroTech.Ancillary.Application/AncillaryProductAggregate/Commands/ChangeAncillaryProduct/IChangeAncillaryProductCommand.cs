using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct
{
    public interface IChangeAncillaryProductCommand
    {
        long AncillaryProductId { get; }

        string Name { get; }

        string? Description { get; }

        AncillarySalesScope SalesScope { get; }

        ProductQuantity Quantity { get; }

        ProductDocument Document { get; }

        ProductCodes Codes { get; }

        ProductTerms Terms { get; }

        AncillaryInventoryControl InventoryControl { get; }

        ProductBaggage? Baggage { get; }

        ProductLounge? Lounge { get; }
    }
}
