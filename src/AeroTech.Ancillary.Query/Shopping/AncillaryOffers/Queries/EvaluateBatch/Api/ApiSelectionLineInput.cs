using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Api;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Api
{
    public sealed class ApiSelectionLineInput
    {
        public string? ClientSelectionId { get; set; }

        public string? OfferItemRef { get; set; }

        public ApiAncillarySelectionInput? Selection { get; set; }

        public IReadOnlyDictionary<string, string>? SelectedOptionRefs { get; set; }
    }
}
