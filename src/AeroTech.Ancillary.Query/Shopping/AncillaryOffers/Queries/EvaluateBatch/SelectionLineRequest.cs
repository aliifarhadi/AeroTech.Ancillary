using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch
{
    public sealed record SelectionLineRequest(
        string? ClientSelectionId,
        string? OfferItemRef,
        AncillarySelection? Selection,
        IReadOnlyDictionary<string, string>? SelectedOptionRefs = null);
}
