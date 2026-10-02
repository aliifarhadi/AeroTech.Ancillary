using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate
{
    public sealed record SoldCombination(
        AncillaryProductType Type,
        AncillarySalesScope SalesScope,
        AncillaryDocumentType DocumentType,
        AncillaryInventoryControl InventoryControl,
        AncillaryQuantityUnit QuantityUnit);
}
