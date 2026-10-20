using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.SeatMaps;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common
{
    public static class OfferPurchase
    {
        private static readonly IReadOnlySet<string> CoverageFields = new HashSet<string>(StringComparer.Ordinal) { "TravellerRef", "BoundRef", "FlightRef", "ChildRef" };

        public static bool IsPriceComplete(PriceAssessment price) => price.Status == PriceAssessmentStatus.Complete && price.IsOrderLevelTotalComplete;

        public static bool CanPurchase(CanonicalAncillaryOfferCandidate candidate)
            => candidate.OfferReadiness == OfferReadiness.Selectable && IsPriceComplete(candidate.Price) && !NeedsCustomerInput(candidate.Selection);

        public static bool CanPurchase(CanonicalAncillarySelectionEvaluation evaluation)
            => evaluation is { Status: SelectionEvaluationStatus.Accepted, ProvisionChangedBySelection: false }
               && evaluation.Candidate.OfferReadiness == OfferReadiness.Selectable
               && IsPriceComplete(evaluation.Candidate.Price)
               && evaluation.Candidate.Price.RequestedQuantityTotal is not null;

        public static bool CanPurchase(SeatTravellerOffer seat)
            => seat.Status == SeatAvailabilityStatus.Available
               && (seat.Candidate is not { } candidate
                   || (candidate.OfferReadiness == OfferReadiness.Selectable && IsPriceComplete(candidate.Price) && candidate.Price.RequestedQuantityTotal is not null));

        private static bool NeedsCustomerInput(CustomerSelectionContract contract)
            => contract.Fields.Any(field => field.Required && !CoverageFields.Contains(field.Name));
    }
}
