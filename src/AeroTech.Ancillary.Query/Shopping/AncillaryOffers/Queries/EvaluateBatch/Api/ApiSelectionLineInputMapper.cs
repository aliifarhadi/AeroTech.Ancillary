using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Api;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Api
{
    public static class ApiSelectionLineInputMapper
    {
        public static IReadOnlyList<SelectionLineRequest>? ToRequests(IReadOnlyList<ApiSelectionLineInput>? inputs)
            => inputs?
                .Select(input => new SelectionLineRequest(
                    input.ClientSelectionId,
                    input.Kind,
                    input.OfferItemRef,
                    input.Selection is null ? null : ApiSelectionInputMapper.ToSelection(input.Selection),
                    input.SeatMapSelectionId))
                .ToList();
    }
}
