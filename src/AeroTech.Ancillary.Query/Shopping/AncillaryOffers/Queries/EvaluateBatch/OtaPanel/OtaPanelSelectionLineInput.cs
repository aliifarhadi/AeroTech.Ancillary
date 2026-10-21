using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.OtaPanel;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.OtaPanel
{
    public sealed class OtaPanelSelectionLineInput
    {
        public string? ClientSelectionId { get; set; }

        public string? OfferItemRef { get; set; }

        public OtaPanelAncillarySelectionInput? Selection { get; set; }

        public IReadOnlyDictionary<string, string>? SelectedOptionRefs { get; set; }
    }
}
