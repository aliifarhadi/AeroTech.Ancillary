using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public static class SellableCandidate
    {
        private static readonly IReadOnlySet<string> GatedVariants = new HashSet<string>(StringComparer.Ordinal)
        {
            AncillaryVariant.MedicalEquipment,
            AncillaryVariant.UnaccompaniedMinor
        };

        public static bool Is(CanonicalAncillaryOfferCandidate candidate, int currencyId)
            => candidate.Profile != AncillaryProfile.Seat
               && !GatedVariants.Contains(candidate.VariantCode)
               && candidate.Confirmation.ConfirmationRequirement == ConfirmationRequirement.Immediate
               && candidate.Availability.InventoryAuthority != InventoryAuthority.Supplier
               && IsSingleUnit(candidate)
               && candidate.Price is
               {
                   Origin: PriceOrigin.Filed,
                   Status: PriceAssessmentStatus.Complete,
                   PricingRateId: > 0,
                   PricingRevisionId: > 0,
                   CompleteUnitTotal: not null,
                   QuoteProviderKey: null,
                   UnappliedFeeLines.Count: 0
               }
               && candidate.Price.CurrencyId == currencyId
               && candidate.Price.FiledCurrencyId == currencyId
               && candidate.Price.PricingUnit != PricingUnit.PerKilogram;

        private static bool IsSingleUnit(CanonicalAncillaryOfferCandidate candidate)
            => candidate.CandidateIdentity.CoverageScope == ServiceCoverageScope.Order
                ? candidate.Travellers.Count == 0 && candidate.CandidateIdentity.TravellerRef.Length == 0
                : candidate.Travellers is [var traveller] && traveller == candidate.CandidateIdentity.TravellerRef;
    }
}
