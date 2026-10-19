using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common
{
    public static class OfferPurchase
    {
        public static bool IsPriceComplete(PriceAssessment price) => price.Status == PriceAssessmentStatus.Complete && price.IsOrderLevelTotalComplete;

        public static bool CanPurchase(CanonicalAncillaryOfferCandidate candidate)
            => candidate.OfferReadiness == OfferReadiness.Selectable && IsPriceComplete(candidate.Price);

        public static bool CanPurchase(CanonicalAncillarySelectionEvaluation evaluation)
            => evaluation is { Status: SelectionEvaluationStatus.Accepted, ProvisionChangedBySelection: false }
               && CanPurchase(evaluation.Candidate)
               && evaluation.Candidate.Price.RequestedQuantityTotal is not null;
    }
}
