using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate
{
    public static class SoldCombinations
    {
        public static IReadOnlyList<SoldCombination> Rows { get; } =
        [
            new SoldCombination(
                AncillaryProductType.ExtraBaggage,
                AncillarySalesScope.TravellerBound,
                AncillaryDocumentType.EmdAssociated,
                AncillaryInventoryControl.Unlimited,
                AncillaryQuantityUnit.Piece),
            new SoldCombination(
                AncillaryProductType.LoungeAccess,
                AncillarySalesScope.TravellerSegment,
                AncillaryDocumentType.EmdStandalone,
                AncillaryInventoryControl.Unlimited,
                AncillaryQuantityUnit.Each)
        ];

        public static void EnsureSold(
            AncillaryProductType type,
            AncillarySalesScope salesScope,
            AncillaryDocumentType documentType,
            AncillaryInventoryControl inventoryControl,
            AncillaryQuantityUnit quantityUnit)
        {
            if (!Rows.Contains(new SoldCombination(type, salesScope, documentType, inventoryControl, quantityUnit)))
                throw ExceptionFactory.AncillaryProductCombinationIsNotSold();
        }
    }
}
