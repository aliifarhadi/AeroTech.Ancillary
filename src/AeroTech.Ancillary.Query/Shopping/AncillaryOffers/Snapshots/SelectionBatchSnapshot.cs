namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record SelectionBatchSnapshot(AncillaryShoppingSnapshot Shopping, IReadOnlyList<SelectionLineSnapshot> Lines, SelectionBasket Basket);
}
