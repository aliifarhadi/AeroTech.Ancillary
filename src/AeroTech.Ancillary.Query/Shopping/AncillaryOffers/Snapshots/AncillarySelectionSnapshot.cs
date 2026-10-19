using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record AncillarySelectionSnapshot(
        AncillaryShoppingSnapshot Shopping,
        AncillaryOfferItem Item,
        CanonicalAncillarySelectionEvaluation Evaluation,
        string CurrentOfferItemRef,
        AncillarySelection Selection,
        IReadOnlyList<OfferOptionGroup> OptionGroups);
}
