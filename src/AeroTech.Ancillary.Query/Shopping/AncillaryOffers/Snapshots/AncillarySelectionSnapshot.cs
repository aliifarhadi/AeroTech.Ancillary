using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record AncillarySelectionSnapshot(
        AncillaryShoppingSnapshot Shopping,
        AncillaryOfferItem Item,
        CanonicalAncillarySelectionEvaluation Evaluation,
        string CurrentOfferItemRef);
}
