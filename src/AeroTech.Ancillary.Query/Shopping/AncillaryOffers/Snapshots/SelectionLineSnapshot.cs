using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record SelectionLineSnapshot(
        string ClientSelectionId,
        SelectionEvaluationStatus Status,
        IReadOnlyList<string> ReasonCodes,
        bool CanPurchase,
        AncillarySelectionSnapshot? Ancillary,
        PricedSelectionToken? PricedSelection)
    {
        public decimal? Total => Ancillary?.Evaluation.Candidate.Price.RequestedQuantityTotal;

        public int? CurrencyId => Ancillary?.Evaluation.Candidate.Price.CurrencyId;

        public CanonicalAncillaryOfferCandidate? Candidate => Ancillary?.Evaluation.Candidate;
    }
}
