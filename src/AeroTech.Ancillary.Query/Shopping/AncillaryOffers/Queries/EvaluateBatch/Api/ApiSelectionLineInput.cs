using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Api;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Api
{
    public sealed class ApiSelectionLineInput
    {
        public string? ClientSelectionId { get; set; }

        public SelectionLineKind? Kind { get; set; }

        public string? OfferItemRef { get; set; }

        public ApiAncillarySelectionInput? Selection { get; set; }

        public IReadOnlyDictionary<string, string>? SelectedOptionRefs { get; set; }

        public string? SeatMapSelectionId { get; set; }

        public bool? ExitRowTermsAccepted { get; set; }
    }
}
