using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Backoffice;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Backoffice
{
    public sealed class BackofficeSelectionLineInput
    {
        public string? ClientSelectionId { get; set; }

        public SelectionLineKind? Kind { get; set; }

        public string? OfferItemRef { get; set; }

        public BackofficeAncillarySelectionInput? Selection { get; set; }

        public IReadOnlyDictionary<string, string>? SelectedOptionRefs { get; set; }

        public string? SeatMapSelectionId { get; set; }

        public bool? ExitRowTermsAccepted { get; set; }
    }
}
