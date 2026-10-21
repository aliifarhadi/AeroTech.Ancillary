using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.OtaPanel;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.OtaPanel
{
    public static class OtaPanelSelectionLineInputMapper
    {
        public static IReadOnlyList<SelectionLineRequest>? ToRequests(IReadOnlyList<OtaPanelSelectionLineInput>? inputs)
            => inputs?
                .Select(input => new SelectionLineRequest(
                    input.ClientSelectionId,
                    input.OfferItemRef,
                    input.Selection is null ? null : OtaPanelSelectionInputMapper.ToSelection(input.Selection),
                    input.SelectedOptionRefs))
                .ToList();
    }
}
