using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.SelectionIds
{
    public sealed record NonSeatSelectionId(
        ShoppingSourceKind SourceKind,
        string SourceRef,
        int? OrderCommercialVersion,
        NonSeatSelectionScope Scope,
        string OfferItemRef,
        AncillaryProfile Profile,
        string VariantCode,
        AncillarySelection Selection);
}
