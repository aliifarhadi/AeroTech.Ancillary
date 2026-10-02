using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct
{
    public interface IDefineAncillaryProductCommand
    {
        int OwnerAirlineId { get; }

        string ProductRef { get; }

        AncillaryProductType Type { get; }

        string Name { get; }

        string? Description { get; }

        AncillarySalesScope SalesScope { get; }

        ProductQuantity Quantity { get; }

        ProductDocument Document { get; }

        ProductCodes Codes { get; }

        ProductTerms Terms { get; }

        AncillaryInventoryControl InventoryControl { get; }

        ProductBaggage? Baggage { get; }
    }
}
