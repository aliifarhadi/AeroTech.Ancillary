using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Backoffice;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Backoffice
{
    public static class BackofficeSelectionLineInputMapper
    {
        public static IReadOnlyList<SelectionLineRequest>? ToRequests(IReadOnlyList<BackofficeSelectionLineInput>? inputs)
            => inputs?
                .Select(input => new SelectionLineRequest(
                    input.ClientSelectionId,
                    input.Kind,
                    input.OfferItemRef,
                    input.Selection is null ? null : BackofficeSelectionInputMapper.ToSelection(input.Selection),
                    input.SeatMapSelectionId,
                    input.ExitRowTermsAccepted,
                    input.SelectedOptionRefs))
                .ToList();
    }
}
